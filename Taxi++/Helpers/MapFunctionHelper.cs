using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Widget;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;
using parser.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Taxi__.Helpers
{
    public class MapFunctionHelper
    {
        string mapkey;
        GoogleMap map;
        public double distance;
        public double duration;
        public string distanceString;
        public string durationstring;

        Android.Gms.Maps.Model.Polyline mPolyline;
        Marker pickupMarker;
        Marker destinationMarker;

        Activity _instance;

        SessionManager sessionManager = SessionManager.GetInstance();

        public MapFunctionHelper(Activity Instance)
        {
            _instance = Instance;
        }

        public MapFunctionHelper(string mMapkey, GoogleMap mmap)
        {
            mapkey = mMapkey;
            map = mmap;
        }

        public string GetGeoCodeUrl(double lat, double lng)
        {
            string url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + mapkey;
            return url;
        }

        public async Task<string> GetGeoJsonAsync(string url)
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                HttpClient client = new HttpClient(handler);
                var result = await client.GetStringAsync(new Uri(url));

                return result;
            }
        }

        public async Task<string> FindCordinateAddress(LatLng position)
        {

            string url = GetGeoCodeUrl(position.Latitude, position.Longitude);
            string json = "";
            string placeAddress = "";

            //Check for Internet connection
            json = await GetGeoJsonAsync(url);

            if (!string.IsNullOrEmpty(json))
            {
                var geoCodeData = JsonConvert.DeserializeObject<GeocodingParser>(json);
                if (!geoCodeData.status.Contains("ZERO"))
                {
                    if (geoCodeData.results[0] != null)
                    {
                        placeAddress = geoCodeData.results[0].formatted_address;
                    }
                }
            }

            return placeAddress;

        }

        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destination)
        {
            //Origin of route
            string str_origin = "origin=" + location.Latitude + "," + location.Longitude;

            //Destination of route
            string str_destination = "destination=" + destination.Latitude + "," + destination.Longitude;

            //mode
            string mode = "mode=driving";

            //Buidling the parameters to the webservice
            string parameters = str_origin + "&" + str_destination + "&" + "&" + mode + "&key=";

            //Output format
            string output = "json";

            string key = mapkey;

            //Building the final url string
            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?" + parameters + key;

            string json = "";
            json = await GetGeoJsonAsync(url);

            return json;

        }

        public void DrawTripOnMap(string json)
        {
            var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            //Decode Encoded Route
            var points = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(points);

            ArrayList routeList = new ArrayList();
            foreach (LatLng item in line)
            {
                routeList.Add(item);
            }

            //Draw Polylines on Map
            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(10)
                .InvokeColor(Color.Blue)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            mPolyline = map.AddPolyline(polylineOptions);

            //Get first point and lastpoint
            LatLng firstpoint = line[0];
            LatLng lastpoint = line[line.Count - 1];

            duration = directionData.routes[0].legs[0].duration.value;
            distance = directionData.routes[0].legs[0].distance.value;
            durationstring = directionData.routes[0].legs[0].duration.text;
            distanceString = directionData.routes[0].legs[0].distance.text;

            //Pickup marker options
            MarkerOptions pickupMarkerOptions = new MarkerOptions();
            pickupMarkerOptions.SetPosition(firstpoint);
            pickupMarkerOptions.SetTitle("My Location");
            pickupMarkerOptions.SetSnippet(durationstring);
            pickupMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));

            //Destination marker options
            MarkerOptions destinationMarkerOptions = new MarkerOptions();
            destinationMarkerOptions.SetPosition(lastpoint);
            destinationMarkerOptions.SetTitle(sessionManager.GetDestination());
            destinationMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

            pickupMarker = map.AddMarker(pickupMarkerOptions);
            
            destinationMarker = map.AddMarker(destinationMarkerOptions);
           
            //Get Trip Bounds
            double southlng = directionData.routes[0].bounds.southwest.lng;
            double southlat = directionData.routes[0].bounds.southwest.lat;
            double northlng = directionData.routes[0].bounds.northeast.lng;
            double northlat = directionData.routes[0].bounds.northeast.lat;

            LatLng southwest = new LatLng(southlat, southlng);
            LatLng northeast = new LatLng(northlat, northlng);
            LatLngBounds tripBound = new LatLngBounds(southwest, northeast);

            CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngBounds(tripBound, 200);
            map.AnimateCamera(cameraUpdate);

            List<Marker> markers = new List<Marker>();
            markers.Add(destinationMarker);
            markers.Add(pickupMarker);

            markers[0].ShowInfoWindow();
            markers[1].ShowInfoWindow();
        }

        public double EstimateFares()
        {
            double basefare = 1.60; //ghana cedis
            const double minfare = 5.00;
            double distanceFare = 0.66; //ghana cedis per kilometer
            double timefare = 0.50; //ghana cedis per minute

            double kmfares = (distance / 1000) * distanceFare;
            double minsfares = (duration / 60) * timefare;

            double amount = kmfares + minsfares + basefare;
            double fares = Math.Floor(amount / 10) * minfare;

            return fares;

        }
    }
}
