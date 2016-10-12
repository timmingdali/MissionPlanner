﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using MissionPlanner;
using MissionPlanner.Utilities;
using MissionPlanner.GCSViews;

using System.Speech.Synthesis;
using System.Speech.Recognition;

using MissionPlanner.Plugin;

// Davis was here
// One or both of these is for HTTP requests. I forget which one
using System.Net;
using System.Net.Http;

//For javascript serializer
using System.Web.Script.Serialization;

using System.IO; // For logging

using System.Threading; // Trololo
using System.Diagnostics; // For stopwatch


/* NOTES TO SELF
 * 
 * 1. All members inherited from abstracts need an "override" tag added in front
 * 2. Basically everything -inherited from abstracts- is temporary right now (eg. "return true")
 * 3. It seems that WebRequest is the right thing to use (ie. it isn't deprecated) THIS IS VERY FALSE
 * 
 */


namespace interoperability
{
    //SDA Classes 
    public class Moving_Obstacle
    {
        //Altitude in feet
        public float altitude_msl { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        //radius in feet
        public float sphere_radius { get; set; }

        public void printall()
        {
            Console.WriteLine("Altitude_MSL: " + altitude_msl + "\nLatitude: " + latitude +
                "\nLongitude: " + longitude + "\nSphere_Radius: " + sphere_radius);
        }
    }

    public class Stationary_Obstacle
    {   //In feet
        public float cylinder_height { get; set; }
        //In feet
        public float cylinder_radius { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }

        public void printall()
        {
            Console.WriteLine("Cylinder Height: " + cylinder_height + "\nLatitude: " + latitude +
                "\nLongitude: " + longitude + "\nCylinder radius: " + cylinder_radius);
        }
    }

    public class Obstacles
    {
        public List<Moving_Obstacle> moving_obstacles;
        public List<Stationary_Obstacle> stationary_obstacles;
    }

    //Mission Classes
    public class Waypoint
    {
        public float altitude_msl { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public int order { get; set; }
        public bool empty { get; set; }

        public Waypoint()
        {
            empty = true;
        }
        public Waypoint(float _latitude, float _longitude)
        {
            latitude = _latitude;
            longitude = _longitude;
            empty = false;
        }
        public Waypoint(double _latitude, double _longitude)
        {
            latitude = (float)_latitude;
            longitude = (float)_longitude;
            empty = false;
        }

        public Waypoint(float _altitude_msl, float _latitude, float _longitude)
        {
            latitude = _latitude;
            longitude = _longitude;
            empty = false;
        }

        public Waypoint(float _altitude_msl, float _latitude, float _longitude, int order)
        {
            latitude = _latitude;
            longitude = _longitude;
            empty = false;
        }
    }

    public class GPS_Position
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public GPS_Position()
        {
            latitude = 0;
            longitude = 0;
        }
        public GPS_Position(float _latitude, float _longitude)
        {
            latitude = _latitude;
            longitude = _longitude;
        }

        public GPS_Position(double _latitude, double _longitude)
        {
            latitude = (float)_latitude;
            longitude = (float)_longitude;
        }
    }

    public class FlyZone
    {
        public float altitude_msl_max { get; set; }
        public float altitude_msl_min { get; set; }
        public List<Waypoint> boundary_pts { get; set; }
        public string name { get; set; }
        public FlyZone(float _altitude_msl_max, float _altitude_msl_min, List<Waypoint> _boundary_pts)
        {
            altitude_msl_max = _altitude_msl_max;
            altitude_msl_min = _altitude_msl_min;
            boundary_pts = _boundary_pts;
            name = "Geofence";
        }
        public FlyZone(float _altitude_msl_max, float _altitude_msl_min, string _name, List<Waypoint> _boundary_pts)
        {
            altitude_msl_max = _altitude_msl_max;
            altitude_msl_min = _altitude_msl_min;
            name = _name;
            boundary_pts = _boundary_pts;           
        }
        public FlyZone()
        {
            altitude_msl_max = 0;
            altitude_msl_min = 0;
            name = "Geofence";
            boundary_pts = new List<Waypoint>();
        }
    }

    //The class that holds a single mission
    public class Mission
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool unedited { get; set; }
        public bool active { get; set; }
        public GPS_Position air_drop_pos { get; set; }
        public GPS_Position emergent_lkp { get; set; }
        public List<FlyZone> fly_zones { get; set; }
        public GPS_Position home_pos { get; set; }
        public List<Waypoint> mission_waypoints { get; set; }
        public GPS_Position off_axis_target_pos { get; set; }
        public List<Waypoint> search_grid_points { get; set; }

        //SRIC removed for AUVSI 2017
        //public GPS_Position sric_pos { get; set; }

        public Mission()
        {
            id = 0;
            name = "New Mission";
            unedited = true;
            active = false;
            air_drop_pos = new GPS_Position();
            emergent_lkp = new GPS_Position();
            fly_zones = new List<FlyZone>();
            fly_zones.Add(new FlyZone());
            home_pos = new GPS_Position();
            mission_waypoints = new List<Waypoint>();
            off_axis_target_pos = new GPS_Position();
            search_grid_points = new List<Waypoint>();
        }
    }

    //Target Classes
    public class Target
    {
        public int id { get; set; }
        public string type { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string orientation { get; set; }
        public float shape { get; set; }
        public float background_color { get; set; }
        public float alphanumeric { get; set; }
        public float alphanumeric_colour { get; set; }
        public float description { get; set; }
        public bool autonomous { get; set; }

        public void printall()
        {
            Console.WriteLine("Target ID: " + id + "\nType: " + type + "\nLatitude: " + latitude +
                "\nLongitude: " + longitude + "\nOrientation: " + orientation + "\nShape: " + shape +
                "\nBackground Colour: " + background_color + "\nAlphanumeric: " + alphanumeric +
                "\nAlphanumeric Colour : " + alphanumeric_colour + "\nDescription: " + description +
                "\nAutonomous: " + autonomous);
        }
    }

    public class Target_List
    {
        public List<Target> List;
    }


    public class Interoperability : Plugin
    {
        //Default credentials if credentials file does not exist
        private string address = "http://192.168.56.101";
        private string username = "testuser";
        private string password = "testpass";

        private string dist_units = "Metres";
        private string airspd_units = "Metres per Second";
        private string geo_cords = "DD.DDDDDD";


        private Thread Telemetry_Thread;
        private Thread Obstacle_SDA_Thread;
        private Thread Mission_Thread;
        private Thread Map_Control_Thread;
        private Thread Callout_Thread;

        private bool Telemetry_Thread_shouldStop = true;        //Used to start/stop the telemtry thread
        private bool Obstacle_SDA_Thread_shouldStop = true;     //Used to start/stop the SDA thread
        private bool Mission_Thread_shouldStop = true;          //Used to start/stop the misison thread
        private bool Map_Control_Thread_shouldStop = true;      //Used to start/stop the map control thread
        private bool Callout_Thread_shouldStop = true;          //Used to start/stop the callout thread

        private bool Telemetry_Thread_isAlive = false;
        private bool Obstacle_SDA_Thread_isAlive = false;
        private bool Mission_Thread_isAlive = false;
        private bool Map_Thread_isAlive = false;
        private bool Callout_Thread_isAlive = false;

        private int ImportantCounter = 0;
        private long ImporantTimeCount = 0;
        private Stopwatch ImportantTimer = new Stopwatch();

        bool Obstacles_Downloaded = false;                  //Used to tell the map control thread we can access obstaclesList 
        bool resetUploadStats = false;                      //Used to reset telemetry upload stats
        bool resetFlightTimer = false;                      //Used to reset the flight timer

        Obstacles obstaclesList;                            //Instance that holds all SDA Obstacles 
        public Interoperability_Settings Settings;          //Instance that holds all Interoperability Settings

        public List<Mission> Mission_List { get; set; }   //Holds a list of all missions from interoperability + Server
        public Mission Current_Mission { get; set; }      //The current mission open in the program  

        private static Interoperability Instance = null;


        public static Mutex Interoperability_Mutex = new Mutex();

        //Instantiate windows forms
        global::Interoperability_GUI_Forms.Interoperability_GUI_Main Interoperability_GUI;

        override public string Name
        {
            get { return ("Interoperability"); }
        }
        override public string Version
        {
            get { return ("0.7.1"); }
        }
        override public string Author
        {
            get { return ("Jesse, Davis, Oliver"); }
        }


        /// <summary>
        /// Run First, checking plugin
        /// </summary>
        /// <returns></returns>
        override public bool Init()
        {
            Instance = this;
            // System.Windows.Forms.MessageBox.Show("Pong");
            Console.Write("* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n"
                + "*                                   UTAT UAV                                  *\n"
                + "*                            Interoperability 0.3.1                           *\n"
                + "* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n");

            //Set up settings object, and load from xml file
            Settings = new Interoperability_Settings();
            Settings.Load();
            getSettings();

            //Instantiate all threads, but do not start
            Telemetry_Thread = new Thread(new ThreadStart(this.Telemetry_Upload));
            Obstacle_SDA_Thread = new Thread(new ThreadStart(this.Obstacle_SDA));
            Mission_Thread = new Thread(new ThreadStart(this.Mission_Download));
            Callout_Thread = new Thread(new ThreadStart(this.Callouts));

            //Instantiate Mission_List
            Mission_List = new List<Mission>();
            Current_Mission = new Mission();

            // Start interface
            Interoperability_GUI = new Interoperability_GUI_Forms.Interoperability_GUI_Main(this.interoperabilityAction, Settings);
            if (Convert.ToBoolean(Settings["showInteroperability_GUI"]) == true)
            {
                Interoperability_GUI.Show();
                //Start map thread
                Map_Control_Thread = new Thread(new ThreadStart(this.Map_Control));
                Map_Control_Thread_shouldStop = false;
                Map_Control_Thread.Start();
            }

            loopratehz = 0.25F;

            //Add item to flight data context menu 
            Host.FDMenuMap.Items.Add(Interoperability_GUI.getContextMenu());
            Host.MainForm.MainMenu.Items.Insert(2, Interoperability_GUI.getMenuStrip());

            // MyView.AddScreen(new MainSwitcher.Screen("FlightData", FlightData, true));
            //Start Important Timer
            ImportantTimer.Start();

            Console.WriteLine("End of init()");

            return (true);
        }
        public static Interoperability getinstance()
        {
            return Instance;
        }

        public void interoperabilityAction(int action)
        {
            switch (action)
            {
                //Start Telemetry_Upload Thread
                case 0:
                    Telemetry_Thread = new Thread(new ThreadStart(this.Telemetry_Upload));
                    Telemetry_Thread_shouldStop = false;
                    Telemetry_Thread.Start();
                    break;
                //Start Obstacle_SDA Thread
                case 1:
                    Obstacle_SDA_Thread = new Thread(new ThreadStart(this.Obstacle_SDA));
                    Obstacle_SDA_Thread_shouldStop = false;
                    Obstacle_SDA_Thread.Start();
                    break;
                //Stop Obstacle_SDA Thread
                case 2:
                    Obstacle_SDA_Thread_shouldStop = true;
                    break;
                //Start mission download thread
                case 3:
                    Mission_Thread = new Thread(new ThreadStart(this.Mission_Download));
                    Mission_Thread_shouldStop = false;
                    Mission_Thread.Start();
                    break;
                //Reset Telemetry Upload Rate Stats
                case 4:
                    resetUploadStats = true;
                    if (!Telemetry_Thread_isAlive)
                    {
                        Interoperability_GUI.setAvgTelUploadText("0Hz");
                        Interoperability_GUI.setUniqueTelUploadText("0Hz");
                        Interoperability_GUI.setTotalTelemUpload(0);
                    }
                    break;
                //Stop telemtry upload thread
                case 5:
                    Telemetry_Thread_shouldStop = true;
                    break;
                //Restart all running threads that rely on server credentials or unit settings
                case 6:
                    getSettings();
                    //No need to reset the map control thread
                    if(Map_Thread_isAlive == false)
                    {
                        Map_Control_Thread_shouldStop = true;
                        Map_Control_Thread = new Thread(new ThreadStart(this.Map_Control));
                        Map_Control_Thread_shouldStop = false;
                        Map_Control_Thread.Start();
                    }
                    //If GUI format is not AUVSI, disable all server threads
                    bool isAUVSI = true;
                    if (Settings["gui_format"] != "AUVSI")
                    {
                        isAUVSI = false;
                        Telemetry_Thread_shouldStop = true;
                        Obstacle_SDA_Thread_shouldStop = true;
                        Mission_Thread_shouldStop = true;
                    }

                    if (Telemetry_Thread_isAlive && isAUVSI)
                    {
                        Telemetry_Thread_shouldStop = true;
                        Telemetry_Thread = new Thread(new ThreadStart(this.Telemetry_Upload));
                        Telemetry_Thread_shouldStop = false;
                        Telemetry_Thread.Start();
                    }
                    if (Obstacle_SDA_Thread_isAlive && isAUVSI)
                    {
                        Obstacle_SDA_Thread_shouldStop = true;
                        Obstacle_SDA_Thread = new Thread(new ThreadStart(this.Obstacle_SDA));
                        Obstacle_SDA_Thread_shouldStop = false;
                        Obstacle_SDA_Thread.Start();
                    }
                    if (Mission_Thread_isAlive && isAUVSI)
                    {
                        Mission_Thread_shouldStop = true;
                        Mission_Thread = new Thread(new ThreadStart(this.Mission_Download));
                        Mission_Thread_shouldStop = false;
                        Mission_Thread.Start();
                    }
                    break;
                //Stop all threads, when loop until all threads are done
                case 7:
                    Telemetry_Thread_shouldStop = true;
                    Mission_Thread_shouldStop = true;
                    Obstacle_SDA_Thread_shouldStop = true;
                    Map_Control_Thread_shouldStop = true;
                    Callout_Thread_shouldStop = true;
                    while (Mission_Thread.IsAlive || Obstacle_SDA_Thread.IsAlive || Telemetry_Thread.IsAlive || Map_Control_Thread.IsAlive || Callout_Thread.IsAlive)
                    {
                        //Wait until all threads have stopped
                    }
                    break;
                //Show the interoperability control panel
                case 8:
                    if (!Interoperability_GUI.isOpened)
                    {
                        Interoperability_GUI = new Interoperability_GUI_Forms.Interoperability_GUI_Main(this.interoperabilityAction, Settings);
                        Interoperability_GUI.Show();
                        //Start map thread
                        Map_Control_Thread = new Thread(new ThreadStart(this.Map_Control));
                        Map_Control_Thread_shouldStop = false;
                        Map_Control_Thread.Start();
                    }
                    else
                    {
                        Interoperability_GUI.BringToFront();
                        if (Interoperability_GUI.WindowState == FormWindowState.Minimized)
                        {
                            Interoperability_GUI.WindowState = FormWindowState.Normal;
                        }
                    }
                    break;
                //Easter egg
                case 9:
                    if (ImportantTimer.ElapsedMilliseconds - ImporantTimeCount > 100)
                    {
                        switch (ImportantCounter)
                        {
                            case 0:
                                Host.MainForm.MainMenu.Items[2].Image = Properties.Resources.Interop_Icon_Oliver;
                                break;
                            case 1:
                                Host.MainForm.MainMenu.Items[2].Image = Properties.Resources.Interop_Icon_Yih_Tang;
                                break;
                            case 2:
                                Host.MainForm.MainMenu.Items[2].Image = Properties.Resources.Interop_Icon_Erik;
                                break;
                            case 3:
                                Host.MainForm.MainMenu.Items[2].Image = Properties.Resources.Interop_Icon_Jesse;
                                break;
                            case 4:
                                Host.MainForm.MainMenu.Items[2].Image = Properties.Resources.Interop_Icon_Rikky;
                                break;
                            default:
                                Host.MainForm.MainMenu.Items[2].Image = Properties.Resources.Interop_Icon;
                                ImportantCounter = -1;
                                break;
                        }
                        ImporantTimeCount = ImportantTimer.ElapsedMilliseconds;
                        ImportantCounter++;
                    }
                    break;
                //Start callout thread
                case 10:
                    Callout_Thread = new Thread(new ThreadStart(this.Callouts));
                    Callout_Thread_shouldStop = false;
                    Callout_Thread.Start();
                    break;
                case 11:
                    resetFlightTimer = true;
                    break;
                //Clear current mission
                case 12:
                    Current_Mission = new Mission();
                    Current_Mission.name = "TEST RESET";
                    break;
                default:
                    break;
            }
        }


        public void test_function()
        {

            //Doesn't seem to work. Need to modify FlightData.cs or ConfigPlanner.cs
            MissionPlanner.Utilities.Settings test = this.Host.config;
            test["CMB_rateattitude"] = "1";
            test.Save();

            //this.Host.FDMenuMap.
        }

        public void getSettings()
        {
            if (Settings.ContainsKey("dist_units") && Settings.ContainsKey("airspd_units") && Settings.ContainsKey("geo_cords"))
            {
                dist_units = Settings["dist_units"];
                airspd_units = Settings["airspd_units"];
                geo_cords = Settings["geo_cords"];
            }
            else
            {
                Settings["dist_units"] = dist_units;
                Settings["airspd_units"] = airspd_units;
                Settings["geo_cords"] = geo_cords;
            }
            if (Settings.ContainsKey("address") && Settings.ContainsKey("username") && Settings.ContainsKey("password"))
            {
                address = Settings["address"];
                username = Settings["username"];
                password = Settings["password"];
            }
            else
            {
                Settings["address"] = address;
                Settings["username"] = username;
                Settings["password"] = password;
            }
            if (!Settings.ContainsKey("showInteroperability_GUI"))
            {
                Settings["showInteroperability_GUI"] = false.ToString();
            }
            Settings.Save();

        }

        public async void Telemetry_Upload()
        {
            Telemetry_Thread_isAlive = true;
            Console.WriteLine("Telemetry_Upload Thread Started");
            Stopwatch t = new Stopwatch();
            t.Start();

            int count = 0;
            CookieContainer cookies = new CookieContainer();

            try
            {
                using (var client = new HttpClient())
                {

                    TimeSpan timeout = new TimeSpan(0, 0, 0, 1);
                    client.Timeout = timeout;

                    client.BaseAddress = new Uri(address); // This seems to change every time

                    // Log in.
                    Console.WriteLine("---INITIAL LOGIN---");
                    var v = new Dictionary<string, string>();
                    v.Add("username", username);
                    v.Add("password", password);
                    var auth = new FormUrlEncodedContent(v);
                    //Get authentication cookie. Cookie is automatically sent after being sent
                    HttpResponseMessage resp = await client.PostAsync("/api/login", auth);
                    Console.WriteLine("Login POST result: " + resp.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("---LOGIN FINISHED---");

                    if (!resp.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Invalid Credentials");
                        Interoperability_GUI.setAvgTelUploadText("Error, Invalid Credentials.");
                        Interoperability_GUI.setUniqueTelUploadText("Error, Invalid Credentials");
                        Interoperability_GUI.TelemResp(resp.Content.ReadAsStringAsync().Result);
                        Interoperability_GUI.Telem_Start_Stop_Button_Off();
                        Telemetry_Thread_shouldStop = true;

                    }
                    else
                    {
                        Console.WriteLine("Credentials Valid");
                        Telemetry_Thread_shouldStop = false;
                    }

                    CurrentState csl = this.Host.cs;
                    double lat = csl.lat, lng = csl.lng, alt = csl.altasl, yaw = csl.yaw;
                    double oldlat = 0, oldlng = 0, oldalt = 0, oldyaw = 0;
                    int uniquedata_count = 0;
                    double averagedata_count = 0;

                    while (Telemetry_Thread_shouldStop == false)
                    {
                        //Doesn't work, need another way to do this
                        //If person sets speed to 0, then GUI crashes 
                        if (Interoperability_GUI.getTelemPollRate() != 0)
                        {
                            csl = this.Host.cs;
                            lat = csl.lat;
                            lng = csl.lng;
                            alt = csl.altasl;
                            yaw = csl.yaw;
                            if (lat != oldlat || lng != oldlng || alt != oldalt || yaw != oldyaw)
                            {
                                uniquedata_count++;
                                averagedata_count++;
                                oldlat = csl.lat;
                                oldlng = csl.lng;
                                oldalt = csl.altasl;
                                oldyaw = csl.yaw;
                            }
                            if (count % Interoperability_GUI.getTelemPollRate() == 0)
                            {
                                Interoperability_GUI.setAvgTelUploadText((averagedata_count / (count / Interoperability_GUI.getTelemPollRate())) + "Hz");
                                Interoperability_GUI.setUniqueTelUploadText(uniquedata_count + "Hz");
                                uniquedata_count = 0;
                            }
                            if (resetUploadStats)
                            {
                                uniquedata_count = 0;
                                averagedata_count = 0;
                                count = 0;
                                resetUploadStats = false;
                            }


                            t.Restart();

                            var telemData = new Dictionary<string, string>();

                            CurrentState cs = this.Host.cs;

                            telemData.Add("latitude", lat.ToString("F10"));
                            telemData.Add("longitude", lng.ToString("F10"));
                            telemData.Add("altitude_msl", alt.ToString("F10"));
                            telemData.Add("uas_heading", yaw.ToString("F10"));
                            //Console.WriteLine("Latitude: " + lat + "\nLongitude: " + lng + "\nAltitude_MSL: " + alt + "\nHeading: " + yaw);

                            var telem = new FormUrlEncodedContent(telemData);
                            HttpResponseMessage telemresp = await client.PostAsync("/api/telemetry", telem);
                            Console.WriteLine("Server_info GET result: " + telemresp.Content.ReadAsStringAsync().Result);
                            Interoperability_GUI.TelemResp(telemresp.Content.ReadAsStringAsync().Result);
                            count++;
                            Interoperability_GUI.setTotalTelemUpload(count);
                        }
                        Thread.Sleep(1000 / Interoperability_GUI.getTelemPollRate());
                    }
                }
            }

            //If this exception is thrown, then the thread will end soon after. Have no way to restart manually unless I get the loop working
            catch//(HttpRequestException)
            {
                //<h1>403 Forbidden</h1> 
                Interoperability_GUI.setAvgTelUploadText("Error, Unable to Connect to Server");
                Interoperability_GUI.setUniqueTelUploadText("Error, Unable to Connect to Server");
                Interoperability_GUI.TelemResp("Error, Unable to Connect to Server");
                Interoperability_GUI.Telem_Start_Stop_Button_Off();
                Console.WriteLine("Error, exception thrown in telemtry upload thread");
            }
            Telemetry_Thread_isAlive = false; 
            Console.WriteLine("Telemetry_Upload Thread Stopped");
            Interoperability_GUI.Telem_Start_Stop_Button_Off();
        }

        //This is where we periodically download the obstacles from the server 
        public async void Obstacle_SDA()
        {
            Obstacle_SDA_Thread_isAlive = true;
            Console.WriteLine("Obstacle_SDA Thread Started");
            Stopwatch t = new Stopwatch();
            t.Start();

            int count = 0;
            CookieContainer cookies = new CookieContainer();

            try
            {
                using (var client = new HttpClient())
                {
                    TimeSpan timeout = new TimeSpan(0, 0, 0, 1);
                    client.Timeout = timeout;
                    client.BaseAddress = new Uri(address); // This seems to change every time

                    // Log in.
                    Console.WriteLine("---INITIAL LOGIN---");
                    var v = new Dictionary<string, string>();
                    v.Add("username", username);
                    v.Add("password", password);
                    var auth = new FormUrlEncodedContent(v);
                    HttpResponseMessage resp = await client.PostAsync("/api/login", auth);
                    Console.WriteLine("Login POST result: " + resp.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("---LOGIN FINISHED---");
                    //resp.IsSuccessStatusCode;
                    if (!resp.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Invalid Credentials");
                        Interoperability_GUI.SDAResp(resp.Content.ReadAsStringAsync().Result);
                        Interoperability_GUI.SetSDAStart_StopButton_Off();
                        Obstacle_SDA_Thread_shouldStop = true;
                    }
                    else
                    {
                        Console.WriteLine("Credentials Valid");
                        Interoperability_GUI.SDAResp(resp.Content.ReadAsStringAsync().Result);
                        Obstacle_SDA_Thread_shouldStop = false;
                    }

                    while (!Obstacle_SDA_Thread_shouldStop)
                    {
                        HttpResponseMessage SDAresp = await client.GetAsync("/api/obstacles");
                        //Console.WriteLine(SDAresp.Content.ReadAsStringAsync().Result);
                        count++;

                        Console.WriteLine("outputting formatted data");
                        obstaclesList = new JavaScriptSerializer().Deserialize<Obstacles>(SDAresp.Content.ReadAsStringAsync().Result);

                        Obstacles_Downloaded = true;
                        Interoperability_GUI.setObstacles(obstaclesList);

                        System.Threading.Thread.Sleep(100);

                        t.Restart();
                        Thread.Sleep(1000 / Interoperability_GUI.getsdaPollRate());
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error, exception thrown in Obstacle_SDA Thread");
                Interoperability_GUI.SDAResp("Error, Unable to Connect to Server");
                Interoperability_GUI.SetSDAStart_StopButton_Off();

            }
            Obstacle_SDA_Thread_isAlive = false;
            Interoperability_GUI.SetSDAStart_StopButton_Off();
            Console.WriteLine("Obstacle_SDA Thread Stopped");
        }

        //Will be used to export to something. Is used to 
        public void ExportMapData(List<Mission> missionList)
        {
            string thing = new JavaScriptSerializer().Serialize(missionList);
            Console.WriteLine(thing);
        }


        public async void Mission_Download()
        {
            Mission_Thread_isAlive = true;
            Console.WriteLine("Mission_Download Thread Started");
            Stopwatch t = new Stopwatch();
            t.Start();
            CookieContainer cookies = new CookieContainer();

            try
            {
                using (var client = new HttpClient())
                {

                    client.BaseAddress = new Uri(address); // This seems to change every time

                    // Log in.
                    var v = new Dictionary<string, string>();
                    v.Add("username", username);
                    v.Add("password", password);
                    var auth = new FormUrlEncodedContent(v);
                    HttpResponseMessage resp = await client.PostAsync("/api/login", auth);
                    if (!resp.IsSuccessStatusCode)
                    {
                        Mission_Thread_shouldStop = true;
                    }
                    else
                    {
                        Console.WriteLine("Credentials Valid");
                        Mission_Thread_shouldStop = false;
                    }

                    while (!Mission_Thread_shouldStop)
                    {
                        HttpResponseMessage SDAresp = await client.GetAsync("/api/missions");
                        Console.WriteLine(SDAresp.Content.ReadAsStringAsync().Result);

                        List<Mission> Server_Mission = new JavaScriptSerializer().Deserialize<List<Mission>>(SDAresp.Content.ReadAsStringAsync().Result);

                        int count = Mission_List.Count();
                        //Add obtained missions to the current list of missions 
                        for(int i=0; i < Server_Mission.Count(); i++)
                        {
                            Server_Mission[i].name = "Server Mission_" + Convert.ToString(i + count);
                            Mission_List.Add(Server_Mission[i]);
                        }
                        
                        Mission_Thread_shouldStop = true;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error, exception thrown in Obstacle_SDA Thread");
            }
            Mission_Thread_isAlive = false;
            Console.WriteLine("Mission_Download Thread Stopped");
        }

        public void Map_Control()
        {
            Map_Thread_isAlive = true;
            Console.WriteLine("Map_Control Thread Started");
            Stopwatch t = new Stopwatch();
            Stopwatch FlightTime = new Stopwatch();
            double GroundElevation;
            t.Start();

            //Add static overlays:
            //For testing right now. Will update when server has misison functionality added
            if (false)
            {
                List<Waypoint> Op_Area = new List<Waypoint>();
                Op_Area.Add(new Waypoint(38.1462694, -76.4277778));
                Op_Area.Add(new Waypoint(38.1516250, -76.4286833));
                Op_Area.Add(new Waypoint(38.1518889, -76.4314667));
                Op_Area.Add(new Waypoint(38.1505944, -76.4353611));
                Op_Area.Add(new Waypoint(38.1475667, -76.4323417));
                Op_Area.Add(new Waypoint(38.1446667, -76.4329472));
                Op_Area.Add(new Waypoint(38.1432556, -76.4347667));
                Op_Area.Add(new Waypoint(38.1404639, -76.4326361));
                Op_Area.Add(new Waypoint(38.1407194, -76.4260139));
                Op_Area.Add(new Waypoint(38.1437611, -76.4212056));
                Op_Area.Add(new Waypoint(38.1473472, -76.4232111));
                Op_Area.Add(new Waypoint(38.1461306, -76.4266528));

                //We clear becaues the construtor creates an empty flyzone.
                Current_Mission.fly_zones.Clear();
                Current_Mission.fly_zones.Add(new FlyZone(600, 100, Op_Area));

                Current_Mission.search_grid_points.Add(new Waypoint(38.1457306, -76.4295972));
                Current_Mission.search_grid_points.Add(new Waypoint(38.1431861, -76.4338917));
                Current_Mission.search_grid_points.Add(new Waypoint(38.1410028, -76.4322333));
                Current_Mission.search_grid_points.Add(new Waypoint(38.1411917, -76.4269806));
                Current_Mission.search_grid_points.Add(new Waypoint(38.1422194, -76.4261111));

                Current_Mission.air_drop_pos = new GPS_Position(38.145852, -76.426416);
                Current_Mission.off_axis_target_pos = new GPS_Position(38.147408, -76.433651);
                Current_Mission.emergent_lkp = new GPS_Position(38.144187, -76.423645);
            }


            List<PointLatLng> Waypoints = new List<PointLatLng>();
            Waypoints.Add(new PointLatLng(38.147720, -76.429610));
            Waypoints.Add(new PointLatLng(38.150893, -76.432056));
            Waypoints.Add(new PointLatLng(38.149559, -76.434159));
            Waypoints.Add(new PointLatLng(38.145729, -76.430275));
            Waypoints.Add(new PointLatLng(38.143147, -76.428344));
            Waypoints.Add(new PointLatLng(38.142168, -76.429482));
            Waypoints.Add(new PointLatLng(38.144176, -76.431584));
            Waypoints.Add(new PointLatLng(38.143214, -76.433151));
            Waypoints.Add(new PointLatLng(38.141898, -76.432593));
            Waypoints.Add(new PointLatLng(38.143063, -76.429675));
            Waypoints.Add(new PointLatLng(38.144531, -76.426542));
            Waypoints.Add(new PointLatLng(38.146471, -76.422379));
            Waypoints.Add(new PointLatLng(38.144801, -76.421757));
            Waypoints.Add(new PointLatLng(38.143029, -76.421692));
            Waypoints.Add(new PointLatLng(38.142084, -76.423817));
            Waypoints.Add(new PointLatLng(38.142016, -76.425469));
            Waypoints.Add(new PointLatLng(38.145189, -76.428537));

            while (!Map_Control_Thread_shouldStop)
            {
                Interoperability_GUI.MAP_Clear_Overlays();
                //Draw Obstacles 
                if (Obstacles_Downloaded)
                {
                    if (Interoperability_GUI.getDrawObstacles())
                    {
                        for (int i = 0; i < obstaclesList.stationary_obstacles.Count(); i++)
                        {
                            Interoperability_GUI.MAP_addSObstaclePoly(obstaclesList.stationary_obstacles[i].cylinder_radius * 0.3048,
                                obstaclesList.stationary_obstacles[i].cylinder_height * 0.3048, obstaclesList.stationary_obstacles[i].latitude,
                                obstaclesList.stationary_obstacles[i].longitude);
                        }

                        for (int i = 0; i < obstaclesList.moving_obstacles.Count(); i++)
                        {
                            Interoperability_GUI.MAP_addMObstaclePoly(obstaclesList.moving_obstacles[i].sphere_radius * 0.3048,
                               obstaclesList.moving_obstacles[i].altitude_msl * 0.3048, obstaclesList.moving_obstacles[i].latitude,
                               obstaclesList.moving_obstacles[i].longitude, "polygon");
                        }
                    }
                }

                //Draw geofence
                if (Interoperability_GUI.getDrawGeofence())
                {
                    //Using first boundary point (assuming there is only one geofence) COME BACK TO THIS LATER
                    Interoperability_GUI.MAP_addStaticPoly(Current_Mission.fly_zones[0].boundary_pts, "Geofence", Color.Red, Color.Transparent, 3, 50);
                }

                //Draw search area
                if (Interoperability_GUI.getDrawSearchArea())
                {
                        Interoperability_GUI.MAP_addStaticPoly(Current_Mission.search_grid_points, "Search_Area", Color.Green, Color.Green, 3, 90); 
                }

                //Draw plane location                   
                if (Interoperability_GUI.getDrawPlane())
                {
                    Interoperability_GUI.MAP_updatePlaneLoc(new PointLatLng(Host.cs.lat, Host.cs.lng), Host.cs.alt, Host.cs.yaw,
                        Host.cs.groundcourse, Host.cs.nav_bearing, Host.cs.target_bearing, Host.cs.radius);
                }

                if (Interoperability_GUI.getDrawWP())
                {
                    //Draw waypoints
                    Interoperability_GUI.MAP_updateWP(Waypoints);
                    //Draw lines between waypoints
                    Interoperability_GUI.MAP_updateWPRoute(Waypoints);
                }
                //Draw off axis targets, emergent targets, and air drop location
                if (Interoperability_GUI.getDrawOFAT_EN_DROP())
                {
                    Interoperability_GUI.MAP_updateOFAT_EN_DROP(Current_Mission);
                }

                if (Interoperability_GUI.getAutopan())
                {
                    Interoperability_GUI.MAP_ChangeLoc(new PointLatLng(Host.cs.lat, Host.cs.lng));
                }
                Interoperability_GUI.MAP_Update_Overlay();


                //Update GPS Location label at bottom of interface
                switch (geo_cords)
                {
                    case "DD.DDDDDD":
                        Interoperability_GUI.MAP_updateGPSLabel(Host.cs.lat.ToString("00.000000") + " " + Host.cs.lng.ToString("00.000000"));
                        break;
                    case "DD MM SS.SS":
                        Interoperability_GUI.MAP_updateGPSLabel(DDtoDMS(Host.cs.lat, Host.cs.lng));
                        break;
                    default:
                        Interoperability_GUI.MAP_updateGPSLabel(Host.cs.lat.ToString("00.000000") + " " + Host.cs.lng.ToString("00.000000"));
                        break;
                }

                GroundElevation = srtm.getAltitude(Host.cs.lat, Host.cs.lng).alt;

                //Update altitude and delta altitude label at bottom of interface
                switch (dist_units)
                {
                    case "Metres":
                        Interoperability_GUI.MAP_updateAltLabel(Host.cs.altasl.ToString("00.000") + "m",
                            (Host.cs.altasl - GroundElevation).ToString("00.000") + "m");
                        break;
                    case "Feet":
                        Interoperability_GUI.MAP_updateAltLabel((3.28084 * Host.cs.altasl).ToString("00.000") + "ft",
                            (3.28084 * Host.cs.altasl - 3.28084 * GroundElevation).ToString("00.000") + "ft");
                        break;
                    default:
                        break;
                }
                Interoperability_GUI.setFlightTimerLabel(FlightTime.ElapsedMilliseconds);
                //Console.WriteLine(srtm.getAltitude(Host.cs.lat, Host.cs.lng).alt.ToString());
                t.Restart();

                //It's okay if we call this multiple times, because it just resumes the flight timer
                if (Host.cs.airspeed > 10)
                {
                    FlightTime.Start();
                }
                else
                {
                    FlightTime.Stop();
                }
                if (resetFlightTimer == true)
                {
                    FlightTime.Reset();
                    resetFlightTimer = false;
                }
                Thread.Sleep(1000 / Interoperability_GUI.getMapRefreshRate());
            }
            Map_Thread_isAlive = false;
            Console.WriteLine("Map_Control Thread Stopped");
        }

        public void Callouts()
        {
            Stopwatch t = new Stopwatch();
            t.Start();
            //Set up speech output 
            SpeechSynthesizer Speech = new SpeechSynthesizer();

            Callout_Thread_isAlive = true;
            Console.WriteLine("Callout Thread Started");

            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
            {
                Choices Modes = new Choices();
                Modes.Add(new string[] { "landing", "taking off", "airspeed", "altitude", "flight time" });

                // Create a GrammarBuilder object and append the Choices object.
                GrammarBuilder gb = new GrammarBuilder();
                gb.Append(Modes);
                Grammar g = new Grammar(gb);
                recognizer.LoadGrammar(g);
                //Voice recognition allows 
                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                recognizer.SetInputToDefaultAudioDevice();

                bool Recognizer_Enabled = false;
                string CalloutMode = "";

                while (Callout_Thread_shouldStop == false)
                {
                    if (Interoperability_GUI.getSpeechRecognition_Enabled() == true && !Recognizer_Enabled)
                    {
                        recognizer.RecognizeAsync(RecognizeMode.Multiple);
                        Recognizer_Enabled = true;
                    }
                    else if (Interoperability_GUI.getSpeechRecognition_Enabled() == false)
                    {
                        recognizer.RecognizeAsyncStop();
                        Recognizer_Enabled = false;
                    }

                    //Probably shouldn't be calling this in an infinite loop
                    CalloutMode = Interoperability_GUI.getCalloutMode();
                    if (CalloutMode == "Landing")
                    {

                    }
                    else if (CalloutMode == "Takeoff")
                    {

                    }
                    //Thread.Sleep(10000000);
                    if (t.ElapsedMilliseconds > (Interoperability_GUI.getCalloutPeriod()))
                    {
                        //If we speak asynchronously, then we might speak over the previous speach  
                        Speech.SpeakAsync(Convert.ToInt32(Host.cs.airspeed).ToString());
                        t.Restart();
                    }
                }

            }
            Console.WriteLine("Callout thread has stopped");
            Callout_Thread_isAlive = false;
        }

        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            MessageBox.Show("Speech recognized: " + e.Result.Text);
        }


        public static string DDtoDMS(double lat, double lng)
        {
            string DMS;

            double minutes = Math.Floor((Math.Abs(lat) - Math.Floor(Math.Abs(lat))) * 60.0);
            double seconds = (Math.Abs(lat) - Math.Floor(Math.Abs(lat)) - minutes / 60) * 3600;
            

            if (lat > 0)
            {
                DMS = "N";
            }
            else
            {
                DMS = "S";
            }
            DMS += Convert.ToInt32(Math.Abs(lat)).ToString("00") + "-" + Math.Abs(minutes).ToString("00") + "-" + Math.Abs(seconds).ToString("00.00") + " ";// + "." + tenths.ToString("00") + " ";

            minutes = Math.Floor((Math.Abs(lng) - Math.Floor(Math.Abs(lng))) * 60.0);
            seconds = (Math.Abs(lng) - Math.Floor(Math.Abs(lng)) - minutes / 60) * 3600;

            if (lng > 0)
            {
                DMS += "E";
            }
            else
            {
                DMS += "W";
            }
            DMS += Convert.ToInt32(Math.Abs(lng)).ToString("000") + "-" + Math.Abs(minutes).ToString("00") + "-" + Math.Abs(seconds).ToString("00.00"); //+ "." + tenths.ToString("00");
            return DMS;
        }

        /// <summary>
        /// Returns Decimal Degrees given a Degree Minute Seconds string
        /// </summary>
        /// <param name="lat">Latitude in DMS</param>
        /// <param name="lng">Longitude in DMS</param>
        /// <returns></returns>
        public static Waypoint DMStoDD(string lat, string lng)
        {
            double DD_Lat, DD_Lng;
            Waypoint Converted_DD = new Waypoint();

            //Assuming the format is correct
            char[] delimiterChars = { '-' };

            string[] lat_split = lat.Split(delimiterChars);
            string[] lng_split = lng.Split(delimiterChars);
            //43.236403, -98.927366
            try
            {
                double lat_degrees = Convert.ToDouble(lat_split[0][1].ToString() + lat_split[0][2].ToString());
                double lat_minutes = Convert.ToDouble(lat_split[1].ToString()) / 60;
                double lat_seconds = Convert.ToDouble(lat_split[2]) / 3600;
                DD_Lat = lat_degrees + lat_minutes + lat_seconds;

                if(lat_split[0][0] == 'S')
                {
                    DD_Lat *= -1;
                }

                double lng_degrees = Convert.ToDouble(lng_split[0][1].ToString() + lng_split[0][2].ToString() + lng_split[0][3].ToString());
                double lng_minutes = Convert.ToDouble(lng_split[1].ToString()) / 60;
                double lng_seconds = Convert.ToDouble(lng_split[2]) / 3600;
                DD_Lng = lng_degrees + lng_minutes + lng_seconds;

                if(lng_split[0][0] == 'W')
                {
                    DD_Lng *= -1;
                }
            }
            catch
            {
                //Something went wrong with the format I think
                return Converted_DD;
            }

            return new Waypoint(DD_Lat, DD_Lng);
        }

        // BE CAREFUL, THIS IS SKETCHY AS FUCK
        // We also don't need this until later :) 
        public /*virtual int*/ async void TrollLoop(/*StreamWriter writer,*/ HttpClient client)
        {
            //Console.WriteLine("LOOP TIME -> " + DateTime.Now.ToString());

            CurrentState cs = this.Host.cs;
            double lat = cs.lat, lng = cs.lng, alt = cs.altasl, yaw = cs.yaw;

            var v = new Dictionary<string, string>();
            v.Add("latitude", lat.ToString("F10"));
            v.Add("longitude", lng.ToString("F10"));
            v.Add("altitude_msl", alt.ToString("F10"));
            v.Add("uas_heading", yaw.ToString("F10"));
            //Console.WriteLine("Latitude: " + lat + "\nLongitude: " + lng + "\nAltitude_MSL: " + alt + "\nHeading: " + yaw);

            var telem = new FormUrlEncodedContent(v);
            HttpResponseMessage telemresp = await client.PostAsync("/api/telemetry", telem);
            Console.WriteLine("Server_info GET result: " + telemresp.Content.ReadAsStringAsync().Result);

            return;
        }

        /// <summary>
        /// Load your own code here, this is only run once on loading
        /// </summary>
        /// <returns></returns>
        override public bool Loaded()
        {
            Console.WriteLine("* * * * * Interoperability plugin loaded. * * * * *");

            // Attempt to login to server?

            return (true);
        }

        /// <summary>
        /// Run at NextRun time - loop is run in a background thread. and is shared with other plugins
        /// Ensure threads that unintentially die are revived
        /// </summary>
        /// <returns></returns>
        override public bool Loop()
        {
            /*if (Telemetry_Upload_shouldStop == false && !Telemetry_Upload_isAlive)
            {
                interoperabilityAction(0);
            }
            if (Obstacle_SDA_shouldStop == false && !Obstacle_SDA_isAlive)
            {
                interoperabilityAction(1);
            }
            if (Map_Control_shouldStop == false && !Map_Control_isAlive)
            {
                interoperabilityAction(6);
            }*/

            return true;
        }

        /// <summary>
        /// Exit is only called on plugin unload. not app exit
        /// </summary>
        /// <returns></returns>
        override public bool Exit()
        {
            interoperabilityAction(7);
            return (true);
        }
    }


    public class Interoperability_Settings
    {
        static Interoperability_Settings _instance;
        private static Mutex Settings_XML_Mutex = new Mutex(); //So we only write settings one at a time;

        public static Interoperability_Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Interoperability_Settings();
                }
                return _instance;
            }
        }

        public Interoperability_Settings()
        {
        }

        /// <summary>
        /// use to store all internal config
        /// </summary>
        public static Dictionary<string, string> config = new Dictionary<string, string>();

        const string FileName = "Interoperability_Config.xml";

        public string this[string key]
        {
            get
            {
                string value = null;
                config.TryGetValue(key, out value);
                return value;
            }

            set
            {
                config[key] = value;
            }
        }

        public IEnumerable<string> Keys
        {
            // the "ToArray" makes this safe for someone to add items while enumerating.
            get { return config.Keys.ToArray(); }
        }
        public bool ContainsKey(string key)
        {
            return config.ContainsKey(key);
        }



        public int Count { get { return config.Count; } }


        internal int GetInt32(string key)
        {
            int result = 0;
            string value = null;
            if (config.TryGetValue(key, out value))
            {
                int.TryParse(value, out result);
            }
            return result;
        }

        internal bool GetBoolean(string key)
        {
            bool result = false;
            string value = null;
            if (config.TryGetValue(key, out value))
            {
                bool.TryParse(value, out result);
            }
            return result;
        }

        internal float GetFloat(string key)
        {
            float result = 0f;
            string value = null;
            if (config.TryGetValue(key, out value))
            {
                float.TryParse(value, out result);
            }
            return result;
        }

        internal double GetDouble(string key)
        {
            double result = 0D;
            string value = null;
            if (config.TryGetValue(key, out value))
            {
                double.TryParse(value, out result);
            }
            return result;
        }

        internal byte GetByte(string key)
        {
            byte result = 0;
            string value = null;
            if (config.TryGetValue(key, out value))
            {
                byte.TryParse(value, out result);
            }
            return result;
        }

        public static string GetFullPath()
        {
            string directory = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.Combine(directory, FileName);
        }

        public void Load()
        {
            if (File.Exists(GetFullPath()))
            {
                Settings_XML_Mutex.WaitOne();
                using (XmlTextReader xmlreader = new XmlTextReader(GetFullPath()))
                {
                    while (xmlreader.Read())
                    {
                        if (xmlreader.NodeType == XmlNodeType.Element)
                        {
                            try
                            {
                                switch (xmlreader.Name)
                                {
                                    case "Config":
                                        break;
                                    case "xml":
                                        break;
                                    default:
                                        config[xmlreader.Name] = xmlreader.ReadString();
                                        break;
                                }
                            }
                            // silent fail on bad entry
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                Settings_XML_Mutex.ReleaseMutex();
            }

        }

        public void Save()
        {
            string filename = GetFullPath();
            Settings_XML_Mutex.WaitOne();
            using (XmlTextWriter xmlwriter = new XmlTextWriter(filename, Encoding.UTF8))
            {
                xmlwriter.Formatting = Formatting.Indented;

                xmlwriter.WriteStartDocument();

                xmlwriter.WriteStartElement("Config");

                foreach (string key in config.Keys)
                {
                    try
                    {
                        if (key == "" || key.Contains("/")) // "/dev/blah"
                            continue;

                        xmlwriter.WriteElementString(key, "" + config[key]);
                    }
                    catch
                    {
                    }
                }

                xmlwriter.WriteEndElement();

                xmlwriter.WriteEndDocument();
                xmlwriter.Close();
            }
            Settings_XML_Mutex.ReleaseMutex();
        }

        public void Remove(string key)
        {
            if (config.ContainsKey(key))
            {
                config.Remove(key);
            }
        }

    }
}