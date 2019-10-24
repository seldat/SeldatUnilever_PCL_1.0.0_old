// Tran Huu Luat 2019 09 07
// Thuật Toán Kiểm Tra Vùng Và Đăng Ký Vùng 
using SeldatMRMS.Management.RobotManagent;
using SeldatMRMS.Management.TrafficManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SeldatMRMS.Management.RobotManagent.RobotUnityControl;
using static SeldatMRMS.ProcedureControlServices;
using static SelDatUnilever_Ver1._00.Management.TrafficManager.TrafficRounterService;

namespace SeldatUnilever_Ver1._02.Management.TrafficManager
{
    public static class TrafficRountineConstants
    {
        // check in c1 là vùng qua ready /gate / Outer
        public enum StateCheckOPZS
        {
            REACHED,
            FORWARD,
            CHECKIN,
        }
        public static RegistryIntersectionZone RegIntZone_READY = new RegistryIntersectionZone("READY");
        public static RegistryIntersectionZone RegIntZone_GATEZONE = new RegistryIntersectionZone("GATE");
        public static RegistryIntersectionZone RegIntZone_MERZ = new RegistryIntersectionZone("MERZ");
        //Ready -> GATE
        public static bool Reg_checkinReady_G12(RobotUnity robot, TrafficManagementService traffic)
        {
            bool onRegG12 = false;
            //kiem tra có robot trong vùng này, nếu có trả về false
            if (traffic.HasOtherRobotUnityinArea("GATE", robot))
            {
                return false;
            }

            if (RegIntZone_GATEZONE.ProcessRegistryIntersectionZone(robot))
            {
                onRegG12 = true;
            }
            if (onRegG12)
            {
                return true;
            }
            RegIntZone_GATEZONE.Release(robot);
            return false;
        }
        // Ready -> OUTER
        public static bool Reg_checkinReady_ReadyandOuter(RobotUnity robot, TrafficManagementService traffic)
        {
            bool onRegReady = false;
            bool onRegGate = false;
            //kiem tra có robot trong vùng này, nếu có trả về false
            if (traffic.HasOtherRobotUnityinArea("READY", robot) || traffic.HasOtherRobotUnityinArea("GATE", robot))
            {
                return false;
            }
            if (RegIntZone_READY.ProcessRegistryIntersectionZone(robot))
            {
                onRegReady = true;
            }
            if (RegIntZone_GATEZONE.ProcessRegistryIntersectionZone(robot))
            {
                onRegGate = true;
            }
            if (onRegReady && onRegGate)
            {
                return true;
            }
            RegIntZone_READY.Release(robot);
            return false;
        }
        // Từ C1 -> GATE
        // Detect no Robot : Ready , G12
        // Check register Elevator -> Ready ->Gate 12
        public static bool Reg_checkinC1_GATE(RobotUnity robot, TrafficManagementService traffic)
        {
            bool onRegReady = false;
            bool onRegGate = false;
            //kiem tra có robot trong vùng này, nếu có trả về false
            //if (traffic.HasOtherRobotUnityinArea("READY", robot) || traffic.HasOtherRobotUnityinArea("GATE", robot))
            if (traffic.HasOtherRobotUnityinArea("READY", robot) || traffic.HasOtherRobotUnityinArea("GATE", robot))
            {
                return false;
            }
            if (RegIntZone_READY.ProcessRegistryIntersectionZone(robot))
            {
                onRegReady = true;
            }
            if (RegIntZone_GATEZONE.ProcessRegistryIntersectionZone(robot))
            {
                onRegGate = true;
            }
            if (onRegReady && onRegGate)
            {
                return true;
            }
            RegIntZone_READY.Release(robot);
            RegIntZone_GATEZONE.Release(robot);
            return false;
        }
        // Từ C1 -> Ready
        // Detect no Robot : Ready
        // Check register Ready
        public static bool Reg_checkinC1_Ready(RobotUnity robot, TrafficManagementService traffic)
        {
            bool onRegReady = false;
            //kiem tra có robot trong vùng này, nếu có trả về false
            if (traffic.HasOtherRobotUnityinArea("READY", robot))
            {
                return false;
            }
            if (RegIntZone_READY.ProcessRegistryIntersectionZone(robot))
            {
                onRegReady = true;
            }
            if (onRegReady)
            {
                return true;
            }
            RegIntZone_READY.Release(robot);
            return false;
        }

        // Từ OUTER -> MERZE
        // Detect no Robot : Ready
        // Check register Ready
        public static bool Reg_checkinMerz(RobotUnity robot, TrafficManagementService traffic)
        {
            bool onRegMerz = false;
            //kiem tra có robot trong vùng này, nếu có trả về false
            if (traffic.HasOtherRobotUnityinArea("MERZ", robot))
            {
                return false;
            }
            if (RegIntZone_MERZ.ProcessRegistryIntersectionZone(robot))
            {
                onRegMerz = true;
            }
            if (onRegMerz)
            {
                return true;
            }
            RegIntZone_MERZ.Release(robot);
            return false;
        }
        public static bool DetectRelease(RegistryRobotJourney rrj)
        {
            // xác định khu vực release
            String destOP = rrj.traffic.DetermineArea(rrj.endPoint, TypeZone.MAIN_ZONE);
            switch (destOP)
            {
                case "OUTER":
                    String startplace = rrj.traffic.DetermineArea(rrj.startPoint, TypeZone.MAIN_ZONE);
                    if (startplace.Equals("VIM"))
                    {
                        // release khi robot vào vùng OUTER
                        if (rrj.traffic.HasRobotUnityinArea("OUTER", rrj.robot))
                        {
                            ReleaseAll(rrj.robot);

                            // rrj.robot.ShowText("RELEASED ROBOT IN REGISTER LIST OF SEPCIAL ZONE FROM VIM -> OUTER");
                            return true;
                        }
                    }
                    break;
                case "VIM":
                    // xác định vùng đến cuối trong VIM.
                    String endPointName = rrj.traffic.DetermineArea(rrj.endPoint, TypeZone.OPZS);
                    if (rrj.traffic.HasRobotUnityinArea(endPointName, rrj.robot))
                    {
                        ReleaseAll(rrj.robot);
                        //  rrj.robot.ShowText("RELEASED ROBOT IN REGISTER LIST OF SEPCIAL ZONE" + endPointName);
                        return true;
                    }
                    break;
            }
            return false;
        }
        public static bool DetetectInsideStationCheck(RegistryRobotJourney rrj)
        {
            // xác định vùng đặt biệt trước khi bắt đầu frontline
            if (rrj.traffic.HasRobotUnityinArea("C1", rrj.robot)
                || rrj.traffic.HasRobotUnityinArea("READY", rrj.robot))
            {
                // Robot được gửi lệnh Stop
                if (StationCheckInSpecialZone(rrj))
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                    return false;
                }
                else
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_STOP, true);
                    ReleaseAll(rrj.robot);
                    return true;
                }
            }
            else
            {
                // Robot van toc bình thuong
                rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                return true;
            }
        }
        public static bool DetetectInsideStationCheckC1ToReady(RegistryRobotJourney rrj)
        {
            // xác định vùng đặt biệt trước khi bắt đầu frontline
            if (rrj.traffic.HasRobotUnityinArea("C1", rrj.robot))
            {
                // Robot được gửi lệnh Stop
                if (Reg_checkinC1_Ready(rrj.robot, rrj.traffic))
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                    return false;
                }
                else
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_STOP, true);
                    ReleaseAll(rrj.robot);
                    return true;
                }
            }
            else
            {
                // Robot van toc bình thuong
                rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                return true;
            }
        }

        public static bool DetetectInsideStationCheckGate(RegistryRobotJourney rrj)
        {
            // xác định vùng đặt biệt trước khi bắt đầu frontline
            if (rrj.traffic.HasRobotUnityinArea("C1", rrj.robot))
            {
                // Robot được gửi lệnh Stop
                if (Reg_checkinC1_GATE(rrj.robot, rrj.traffic)  )
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                    return false;
                }
                else
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_STOP, true);
                    ReleaseAll(rrj.robot);
                    return true;
                }
            }
            else
            {
                // Robot van toc bình thuong
                rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                return true;
            }
        }

  
        public static bool DetetectInsideStationCheckMerz(RegistryRobotJourney rrj)
        {
            // xác định vùng đặt biệt trước khi bắt đầu frontline
            if (rrj.traffic.HasRobotUnityinArea("CMERZ", rrj.robot))
            {
                // Robot được gửi lệnh Stop
                if (Reg_checkinMerz(rrj.robot, rrj.traffic))
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                    return false;
                }
                else
                {
                    rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_STOP, true);
                    ReleaseAll(rrj.robot);
                    return true;
                }
            }
            else
            {
                // Robot van toc bình thuong
                rrj.robot.SetSpeedRegZone(RobotSpeedLevel.ROBOT_SPEED_NORMAL, false);
                return true;
            }
        }
        public static bool StationCheckInSpecialZone(RegistryRobotJourney rrj)
        {
            // vì "OUTER" có kiều là Main_Zone, nhưng vùng khac co kieu là OPZS
            String startZone = rrj.traffic.DetermineArea(rrj.startPoint, TypeZone.MAIN_ZONE).Equals("OUTER") ? "OUTER" : rrj.traffic.DetermineArea(rrj.startPoint, TypeZone.OPZS);
            String endZone = rrj.traffic.DetermineArea(rrj.endPoint, TypeZone.MAIN_ZONE).Equals("OUTER") ? "OUTER" : rrj.traffic.DetermineArea(rrj.endPoint, TypeZone.OPZS);
            rrj.robot.StartPointName = startZone;
            rrj.robot.EndPointName = endZone;
            #region READY -> GATE, OUTER
            if (startZone.Equals("READY") && endZone.Equals("GATE"))
            {
                return Reg_checkinReady_G12(rrj.robot, rrj.traffic);
            }
           
            if (startZone.Equals("READY") && endZone.Equals("OUTER"))
            {
                return Reg_checkinReady_ReadyandOuter(rrj.robot, rrj.traffic);
            }
            #endregion

            #region OUTER (C1) -> READY , GATE, ELEVATOR, GATE3, VIM
            if (startZone.Equals("OUTER") && endZone.Equals("GATE"))
            {
                return Reg_checkinC1_GATE(rrj.robot, rrj.traffic);
            }
            // OUTER->READY
            if (startZone.Equals("OUTER") && endZone.Equals("READY"))
            {
                return Reg_checkinC1_Ready(rrj.robot, rrj.traffic);
            }
            #endregion

            #region ANY_PLACE -> MERZ
            if ( endZone.Equals("MERZ"))
            {
                return Reg_checkinMerz(rrj.robot, rrj.traffic);
            }
            
            #endregion
            return false;
        }
        public static void ReleaseAll(RobotUnity robot)
        {
            RegIntZone_READY.Release(robot);
            RegIntZone_GATEZONE.Release(robot);
        }
        public static void ReleaseAll(RobotUnity robot,String nameException)
        {
            RegIntZone_READY.Release(robot);
            RegIntZone_GATEZONE.Release(robot);
        }

    }
}
