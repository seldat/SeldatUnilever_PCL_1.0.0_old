﻿using SeldatMRMS.Management.DoorServices;
using SeldatUnilever_Ver1._02;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SeldatMRMS
{
    public static class Global_Object
    {

        public enum PRIORITYLOGIN
        {
            PRIORITYLOGIN_ADMIN0=0,
            PRIORITYLOGIN_ADMIN1 = 1,
            PRIORITYLOGIN_GUEST = 2,
        }
        //#######################################
        public static bool IsEngLish = false;


        public static string url = @"http://localhost:8081/robot/rest/";
        //public static string url = @"http://192.168.1.35:8081/robot/rest/";
        // public static string url = @"http://192.168.1.22:8081/robot/rest/";

        public static int userLogin = -2;
        public static string userName = "";
        public static int userAuthor = -2;
        public static bool onFlagRobotComingGateBusy = false;
        public static bool [] FlagGateBusy= new bool[2]{ false,false }; // gate1
        public static bool onFlagDoorBusy_G2 = false; // gate2
        public static bool onFlagDoorBusy = false;
        public static bool onAcceptDevice =true;

        public static string messageDuplicated = "{0} is duplicated.";
        public static string messageSaveSucced = "Save operation succeeded.";
        public static string messageSaveFail = "Failed to save. Please try again.";
        public static string messageValidate = "{0} is mandatory. Please enter {1}.";
        public static string messageNothingSelected = "Nothing selected.";
        public static string messageDeleteConfirm = "Do you want to delete the selected {0}?";
        public static string messageDeleteSucced = "Delete operation succeeded.";
        public static string messageDeleteFail = "Failed to delete. Please try again.";
        public static string messageDeleteUse = "Can\'t delete {0} because it has been using on {1}.";
        public static string messageValidateNumber = "{0} must be {1} than {2}.";
        public static string messageNoDataSave = "There is no updated data to save.";


        public static string messageTitileInformation = "Information";
        public static string messageTitileError = "Error";
        public static string messageTitileWarning = "Warning";
        public static int cntGoready = 1;
        public static int cntForkLiftToBuffer = 0; //
        public static int cntBufferToMachine = 0;

        public static DateTime startTimeProgram ;

        public static DoorManagementService doorManagementServiceCtrl;
        public static MainWindow mainWindowCtrl;

        public static bool getGateStatus(int id) // gate {1,2} ; FlagGateBusy-Array: 0, 1
        {
            return FlagGateBusy[id-1];
        }
        public static void setGateStatus(int id, bool status)
        {
            if ((id - 1) < 0)
                return;
            FlagGateBusy[id-1]=status;
        }
        //#######################################
        public static bool ServerAlive(string hostUri = "localhost", int portNumber = 8081)
        {
            //=====================================================
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect("localhost", 8081);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            //=====================================================
            //try
            //{
            //    using (var client = new TcpClient(hostUri, portNumber))
            //    {
            //        return true;
            //    }
            //}
            //catch (SocketException ex)
            //{
            //    return false;
            //}
            //=====================================================
        }
        //#######################################
        public static MusicPlayerOld musicPlayerOld;
        public static void PlayWarning()
        {
            String path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Resources/Alarm_EM.mp3");
            if (musicPlayerOld == null)
            {
                musicPlayerOld = new MusicPlayerOld(path);
                musicPlayerOld.Play(true);
            }
            else
            {
                if (musicPlayerOld.IsBeingPlayed)
                {
                    return;
                }
                //if (Global_Object.musicPlayerOld.IsBeingPlayed)
                //{
                //    Global_Object.musicPlayerOld.StopPlaying();
                //}
                else
                {
                    musicPlayerOld.Play(true);
                }
            }
        }
        public static void StopWarning()
        {
            if (musicPlayerOld != null)
            {
                if (musicPlayerOld.IsBeingPlayed)
                {
                    musicPlayerOld.StopPlaying();
                }
            }
        }

        //#######################################

        //#######################################
        
        public static Point LaserOriginalCoor = new Point(548, 406);
      //  public static Point LaserOriginalCoor = new Point(740, 406);
        public static Point OriginPoint = new Point(0, 0);
        public static Point CoorLaser(Point canvas)
        {
            Point laser = new Point();
            laser.X = (Math.Cos(0) * (canvas.X - OriginPoint.X)) * resolution;
            laser.Y = (Math.Cos(Math.PI) * (canvas.Y - OriginPoint.Y)) * resolution;
            return laser;
        }

        public static Point CoorCanvas(Point laser)
        {
            Point canvas = new Point();
            canvas.X = (laser.X / (resolution * Math.Cos(0))) + OriginPoint.X;
            canvas.Y = (laser.Y / (resolution * Math.Cos(Math.PI))) + OriginPoint.Y;
            return canvas;
        }

        public static DataTable LoadExcelFile()
        {
            DataTable data = new DataTable();
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Excel files (*.xls)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 4;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    string filePath = openFileDialog.FileName;
                    Console.WriteLine(filePath);
                    //Read the contents of the file into a stream
                    //var fileStream = openFileDialog.OpenFile();
                    //using (StreamReader reader = new StreamReader(fileStream))
                    //{
                    //    string fileContent = reader.ReadToEnd();
                    //    Console.WriteLine(fileContent);
                    //}
                    string name = "Sheet1";
                    string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                    filePath +
                                    ";Extended Properties='Excel 12.0 XML;HDR=YES;';";

                    OleDbConnection con = new OleDbConnection(constr);
                    OleDbCommand oconn = new OleDbCommand("Select * From [" + name + "$]", con);
                    con.Open();
                    OleDbDataAdapter sda = new OleDbDataAdapter(oconn);
                    sda.Fill(data);
                }
                return data;
            }
        }

        public static DataTable LoadExcelFile(string path)
        {
            DataTable data = new DataTable();
            string name = "Sheet1";
            string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            path +
                            ";Extended Properties='Excel 12.0 XmL;HDR=YES;';";

            OleDbConnection con = new OleDbConnection(constr);
            OleDbCommand oconn = new OleDbCommand("Select * From [" + name + "$]", con);
            con.Open();
            OleDbDataAdapter sda = new OleDbDataAdapter(oconn);
            sda.Fill(data);
            return data;

        }

        public static double resolution = 0.1; // Square meters per pixel
        public static string Foo<T>(T parameter) { return typeof(T).Name; }
        public static double palletWidth = 13;
        public static double palletHeight = 15;
        public static double LengthBetweenPoints(Point pt1, Point pt2)
        {
            //Calculate the distance between the both points
            //for both axes separately.
            double dblDistX = Math.Abs(pt1.X - pt2.X);
            double dblDistY = Math.Abs(pt1.Y - pt2.Y);

            //Calculate the length of a line traveling from pt1 to pt2
            //(according to Pythagoras).
            double dblHypotenuseLength = Math.Sqrt(
               dblDistX * dblDistX
               +
               dblDistY * dblDistY
             );

            return dblHypotenuseLength;
        }

    }
}