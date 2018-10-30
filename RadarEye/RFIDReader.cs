using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;

namespace RadarEye
{
    class RFIDReader
    {
        static int reportCount = 0;
        static int eventCount = 0;
        LLRPClient reader;
        String readerName = "192.168.10.123";
        // String readerName = "192.168.0.100";
        static List<String> tagMessageList = new List<string>();
        public TagReportListener listener;
        // Simple Handler for receiving the tag reports from the reader
        void reader_OnRoAccessReportReceived(MSG_RO_ACCESS_REPORT msg)
        {
            // Report could be empty
            if (msg.TagReportData == null) return;

            // Loop through and print out each tag
            for (int i = 0; i < msg.TagReportData.Length; i++)
            {
                reportCount++;

                // just write out the EPC as a hex string for now. It is guaranteed to be
                // in all LLRP reports regardless of default configuration
                string epc;
                if (msg.TagReportData[i].EPCParameter[0].GetType() == typeof(PARAM_EPC_96))
                {
                    epc = ((PARAM_EPC_96)(msg.TagReportData[i].EPCParameter[0])).EPC.ToHexString();
                }
                else
                {
                    epc = ((PARAM_EPCData)(msg.TagReportData[i].EPCParameter[0])).EPC.ToHexString();
                }
                int antennaId = msg.TagReportData[i].AntennaID.AntennaID;
                int channelIndex = 0;//msg.TagReportData[i].ChannelIndex.ChannelIndex;
                int inventoryParameterSpecID = msg.TagReportData[i].InventoryParameterSpecID.InventoryParameterSpecID;

                long time = (long)msg.TagReportData[i].FirstSeenTimestampUTC.Microseconds;
                int phase = 0;
                int rssi = 0;
                int doppler = 0;

                // just write out the EPC as a hex string for now. It is guaranteed to be
                // in all LLRP reports regardless of default configuration
                if (msg.TagReportData[i].AccessCommandOpSpecResult.Count != 0)
                {
                    PARAM_C1G2WriteOpSpecResult result =
                      (PARAM_C1G2WriteOpSpecResult)
                        msg.TagReportData[i].AccessCommandOpSpecResult[0];
                    Console.WriteLine(result.NumWordsWritten +
                        " words written to tag with EPC = " + epc);
                }

                // check for other Impinj Specific tag data and print it out 
                if (msg.TagReportData[i].Custom != null)
                {
                    for (int x = 0; x < msg.TagReportData[i].Custom.Count; x++)
                    {

                        if (msg.TagReportData[i].Custom[x].GetType() ==
                            typeof(PARAM_ImpinjSerializedTID))
                        {
                            PARAM_ImpinjSerializedTID stid =
                                (PARAM_ImpinjSerializedTID)msg.TagReportData[i].Custom[x];


                        }
                        if (msg.TagReportData[i].Custom[x].GetType() ==
                           typeof(PARAM_ImpinjPeakRSSI))
                        {
                            PARAM_ImpinjPeakRSSI Prssi =
                                (PARAM_ImpinjPeakRSSI)msg.TagReportData[i].Custom[x];

                            rssi = Prssi.RSSI;
                        }

                        if (msg.TagReportData[i].Custom[x].GetType() ==
                          typeof(PARAM_ImpinjRFDopplerFrequency))
                        {
                            PARAM_ImpinjRFDopplerFrequency DopplerFrequency =
                                (PARAM_ImpinjRFDopplerFrequency)msg.TagReportData[i].Custom[x];

                            doppler = DopplerFrequency.DopplerFrequency;

                        }
                        if (msg.TagReportData[i].Custom[x].GetType() ==
                          typeof(PARAM_ImpinjRFPhaseAngle))
                        {
                            PARAM_ImpinjRFPhaseAngle RFPhaseAngle =
                                (PARAM_ImpinjRFPhaseAngle)msg.TagReportData[i].Custom[x];

                            phase = RFPhaseAngle.PhaseAngle;

                        }
                    }
                }
                listener.tagReport(epc, antennaId, channelIndex, doppler, rssi, phase, time, inventoryParameterSpecID);
                tagMessageList.Add(epc + " " + time);
                // Console.WriteLine(epc);
            }
        }

        // Simple Handler for receiving the reader events from the reader
        void reader_OnReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg)
        {
            // Events could be empty
            if (msg.ReaderEventNotificationData == null) return;

            // Just write out the LTK-XML for now
            eventCount++;

            // speedway readers always report UTC timestamp
            UNION_Timestamp t = msg.ReaderEventNotificationData.Timestamp;
            PARAM_UTCTimestamp ut = (PARAM_UTCTimestamp)t[0];
            double millis = (ut.Microseconds + 500) / 1000;

            // LLRP reports time in microseconds relative to the Unix Epoch
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = epoch.AddMilliseconds(millis);

            Console.WriteLine("======Reader Event " + eventCount.ToString() + "======" +
                now.ToString("O"));

            // this is how you would look for individual events of interest
            // Here I just dump the event
            if (msg.ReaderEventNotificationData.AISpecEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.AISpecEvent.ToString());
            if (msg.ReaderEventNotificationData.AntennaEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.AntennaEvent.ToString());
            if (msg.ReaderEventNotificationData.ConnectionAttemptEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ConnectionAttemptEvent.ToString());
            if (msg.ReaderEventNotificationData.ConnectionCloseEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ConnectionCloseEvent.ToString());
            if (msg.ReaderEventNotificationData.GPIEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.GPIEvent.ToString());
            if (msg.ReaderEventNotificationData.HoppingEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.HoppingEvent.ToString());
            if (msg.ReaderEventNotificationData.ReaderExceptionEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReaderExceptionEvent.ToString());
            if (msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent.ToString());
            if (msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent.ToString());
            if (msg.ReaderEventNotificationData.ROSpecEvent != null)
            {
                // Console.WriteLine(msg.ReaderEventNotificationData.ROSpecEvent.ToString());
                Console.WriteLine((DateTime.Now.TimeOfDay.ToString()));
            }




        }

        // Duplicate the above handler for encapsulated reader event notifications
        // You would use encapsulated notifications if you wanted to use the same
        // static handler for multiple readers
        void reader_OnEncapedReaderEventNotification(ENCAPED_READER_EVENT_NOTIFICATION enc)
        {
            MSG_READER_EVENT_NOTIFICATION msg = enc.ntf;

            // Events could be empty
            if (msg.ReaderEventNotificationData == null) return;

            // Just write out the LTK-XML for now
            eventCount++;

            // speedway readers always report UTC timestamp
            UNION_Timestamp t = msg.ReaderEventNotificationData.Timestamp;
            PARAM_UTCTimestamp ut = (PARAM_UTCTimestamp)t[0];
            double millis = (ut.Microseconds + 500) / 1000;

            // LLRP reports time in microseconds relative to the Unix Epoch
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = epoch.AddMilliseconds(millis);

            Console.WriteLine("======Reader Event from " + enc.reader + " " + eventCount.ToString() + "======" +
                now.ToString("O"));

            // this is how you would look for individual events of interest
            // Here I just dump the event
            if (msg.ReaderEventNotificationData.AISpecEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.AISpecEvent.ToString());
            if (msg.ReaderEventNotificationData.AntennaEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.AntennaEvent.ToString());
            if (msg.ReaderEventNotificationData.ConnectionAttemptEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ConnectionAttemptEvent.ToString());
            if (msg.ReaderEventNotificationData.ConnectionCloseEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ConnectionCloseEvent.ToString());
            if (msg.ReaderEventNotificationData.GPIEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.GPIEvent.ToString());
            if (msg.ReaderEventNotificationData.HoppingEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.HoppingEvent.ToString());
            if (msg.ReaderEventNotificationData.ReaderExceptionEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReaderExceptionEvent.ToString());
            if (msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReportBufferLevelWarningEvent.ToString());
            if (msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ReportBufferOverflowErrorEvent.ToString());
            if (msg.ReaderEventNotificationData.ROSpecEvent != null)
                Console.WriteLine(msg.ReaderEventNotificationData.ROSpecEvent.ToString());



        }

        void usage()
        {
            Console.WriteLine("usage: docsample2.exe <readerName|IP");
            return;
        }
        public void Initializing()
        {
            Console.WriteLine("Initializing\n");

            //Create an instance of LLRP reader client.
            reader = new LLRPClient();

            //Impinj Best Practice! Always Install the Impinj extensions
            Impinj_Installer.Install();

            Console.WriteLine("Adding Event Handlers\n");
            reader.OnReaderEventNotification += new delegateReaderEventNotification(reader_OnReaderEventNotification);
            reader.OnRoAccessReportReceived += new delegateRoAccessReport(reader_OnRoAccessReportReceived);


        }
        public void Connecting()
        {
            #region Connecting
            {
                Console.WriteLine("Connecting To Reader\n");

                ENUM_ConnectionAttemptStatusType status;

                //Open the reader connection.  Timeout after 5 seconds
                bool ret = reader.Open(readerName, 5000, out status);

                //Ensure that the open succeeded and that the reader
                // returned the correct connection status result

                if (!ret || status != ENUM_ConnectionAttemptStatusType.Success)
                {
                    Console.WriteLine("Failed to Connect to Reader \n");
                    throw new Exception("Failed to Connect to Reader");


                }
            }
            #endregion
            MSG_GET_READER_CAPABILITIES msg = new MSG_GET_READER_CAPABILITIES();
            msg.RequestedData = ENUM_GetReaderCapabilitiesRequestedData.LLRP_Capabilities;
            MSG_ERROR_MESSAGE msg_err;
            MSG_GET_READER_CAPABILITIES_RESPONSE msg_rsp = reader.GET_READER_CAPABILITIES(msg, out msg_err, 8000);
            Console.WriteLine(msg.ToString());

        }
        public void ConfigReader()
        {
            #region EnableExtensions
            {
                Console.WriteLine("Enabling Impinj Extensions\n");

                MSG_IMPINJ_ENABLE_EXTENSIONS imp_msg =
                                                new MSG_IMPINJ_ENABLE_EXTENSIONS();
                MSG_ERROR_MESSAGE msg_err;

                imp_msg.MSG_ID = 1; // not this doesn't need to bet set as the library will default

                //Send the custom message and wait for 8 seconds
                MSG_CUSTOM_MESSAGE cust_rsp = reader.CUSTOM_MESSAGE(imp_msg, out msg_err, 8000);
                MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE msg_rsp =
                    cust_rsp as MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE;

                if (msg_rsp != null)
                {
                    if (msg_rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                    {
                        Console.WriteLine(msg_rsp.LLRPStatus.StatusCode.ToString());
                        reader.Close();
                        return;
                    }
                }
                else if (msg_err != null)
                {
                    Console.WriteLine(msg_err.ToString());
                    reader.Close();
                    return;
                }
                else
                {
                    Console.WriteLine("Enable Extensions Command Timed out\n");
                    reader.Close();
                    return;
                }
            }
            #endregion

            #region FactoryDefault
            {
                Console.WriteLine("Factory Default the Reader\n");

                // factory default the reader
                MSG_SET_READER_CONFIG msg_cfg = new MSG_SET_READER_CONFIG();
                MSG_ERROR_MESSAGE msg_err;

                msg_cfg.ResetToFactoryDefault = true;
                msg_cfg.MSG_ID = 2; //this doesn't need to bet set as the library will default

                //if SET_READER_CONFIG affects antennas it could take several seconds to return
                MSG_SET_READER_CONFIG_RESPONSE rsp_cfg = reader.SET_READER_CONFIG(msg_cfg, out msg_err, 12000);

                if (rsp_cfg != null)
                {
                    if (rsp_cfg.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                    {
                        Console.WriteLine(rsp_cfg.LLRPStatus.StatusCode.ToString());
                        reader.Close();
                        return;
                    }
                }
                else if (msg_err != null)
                {
                    Console.WriteLine(msg_err.ToString());
                    reader.Close();
                    return;
                }
                else
                {
                    Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                    reader.Close();
                    return;
                }
            }
            #endregion

            #region SetReaderConfig
            {
                Console.WriteLine("Adding SET_READER_CONFIG n");

                // Communicate that message to the reader
                MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();


                msg.ResetToFactoryDefault = false;

                // turn off all reports 
                msg.ROReportSpec = new PARAM_ROReportSpec();
                msg.ROReportSpec.TagReportContentSelector = new PARAM_TagReportContentSelector();
                msg.ROReportSpec.TagReportContentSelector.EnableAccessSpecID = true;
                msg.ROReportSpec.TagReportContentSelector.EnableAntennaID = true;
                msg.ROReportSpec.TagReportContentSelector.EnableChannelIndex = true;
                msg.ROReportSpec.TagReportContentSelector.EnableFirstSeenTimestamp = true;
                msg.ROReportSpec.TagReportContentSelector.EnableInventoryParameterSpecID = true;
                msg.ROReportSpec.TagReportContentSelector.EnableLastSeenTimestamp = true;
                msg.ROReportSpec.TagReportContentSelector.EnablePeakRSSI = true;
                msg.ROReportSpec.TagReportContentSelector.EnableROSpecID = true;
                msg.ROReportSpec.TagReportContentSelector.EnableSpecIndex = true;
                msg.ROReportSpec.TagReportContentSelector.EnableTagSeenCount = true;

                // report all tags immediately 
                msg.ROReportSpec.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;
                msg.ROReportSpec.N = 1;

                msg.ReaderEventNotificationSpec = new PARAM_ReaderEventNotificationSpec();
                msg.ReaderEventNotificationSpec.EventNotificationState = new PARAM_EventNotificationState[1];
                msg.ReaderEventNotificationSpec.EventNotificationState[0] = new PARAM_EventNotificationState();
                msg.ReaderEventNotificationSpec.EventNotificationState[0].EventType = ENUM_NotificationEventType.ROSpec_Event;
                msg.ReaderEventNotificationSpec.EventNotificationState[0].NotificationState = true;
                PARAM_ImpinjTagReportContentSelector impinjTagData = new PARAM_ImpinjTagReportContentSelector();
                impinjTagData.ImpinjEnableGPSCoordinates = new PARAM_ImpinjEnableGPSCoordinates();
                impinjTagData.ImpinjEnableGPSCoordinates.GPSCoordinatesMode = ENUM_ImpinjGPSCoordinatesMode.Enabled;
                impinjTagData.ImpinjEnablePeakRSSI = new PARAM_ImpinjEnablePeakRSSI();
                impinjTagData.ImpinjEnablePeakRSSI.PeakRSSIMode = ENUM_ImpinjPeakRSSIMode.Enabled;
                impinjTagData.ImpinjEnableRFPhaseAngle = new PARAM_ImpinjEnableRFPhaseAngle();
                impinjTagData.ImpinjEnableRFPhaseAngle.RFPhaseAngleMode = ENUM_ImpinjRFPhaseAngleMode.Enabled;
                impinjTagData.ImpinjEnableRFDopplerFrequency = new PARAM_ImpinjEnableRFDopplerFrequency();
                impinjTagData.ImpinjEnableRFDopplerFrequency.RFDopplerFrequencyMode = ENUM_ImpinjRFDopplerFrequencyMode.Enabled;

                msg.ROReportSpec.Custom.Add(impinjTagData);



                MSG_ERROR_MESSAGE msg_err;

                MSG_SET_READER_CONFIG_RESPONSE rsp = reader.SET_READER_CONFIG(msg, out msg_err, 12000);
                if (rsp != null)
                {
                    if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                    {
                        Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                        reader.Close();
                        return;
                    }
                }
                else if (msg_err != null)
                {
                    Console.WriteLine(msg_err.ToString());
                    reader.Close();
                    return;
                }
                else
                {
                    Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                    reader.Close();
                    return;
                }
            }
            #endregion
        }
        public void ADDRoSpec()
        {
            Console.WriteLine("Adding RoSpec\n");

            // set up the basic parameters in the ROSpec. Use all the defaults from the reader
            MSG_ADD_ROSPEC msg = new MSG_ADD_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpec = new PARAM_ROSpec();
            msg.ROSpec.CurrentState = ENUM_ROSpecState.Disabled;
            msg.ROSpec.Priority = 0x00;
            msg.ROSpec.ROSpecID = 1111;

            // setup the start and stop triggers in the Boundary Spec
            msg.ROSpec.ROBoundarySpec = new PARAM_ROBoundarySpec();
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Null;

            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger();
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger.ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null;
            msg.ROSpec.ROBoundarySpec.ROSpecStopTrigger.DurationTriggerValue = 0; // ignored by reader

            // Add a single Antenna Inventory to the ROSpec
            msg.ROSpec.SpecParameter = new UNION_SpecParameter();
            PARAM_AISpec aiSpec = new PARAM_AISpec();

            aiSpec.AntennaIDs = new UInt16Array();
            aiSpec.AntennaIDs.Add(1);
            aiSpec.AntennaIDs.Add(2);
            aiSpec.AISpecStopTrigger = new PARAM_AISpecStopTrigger();
            aiSpec.AISpecStopTrigger.AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Null;

            // use all the defaults from the reader.  Just specify the minimum required
            aiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[1];
            aiSpec.InventoryParameterSpec[0] = new PARAM_InventoryParameterSpec();
            aiSpec.InventoryParameterSpec[0].InventoryParameterSpecID = 1234;
            aiSpec.InventoryParameterSpec[0].ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2;
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration = new PARAM_AntennaConfiguration[1];
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration[0] = new PARAM_AntennaConfiguration();
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration[0].AntennaID = 0;
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration[0].RFTransmitter = new PARAM_RFTransmitter();
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration[0].RFTransmitter.ChannelIndex = 1;
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration[0].RFTransmitter.TransmitPower = 91;
            // Add the inventory command to the AI spec.
            PARAM_C1G2InventoryCommand c1g2Inv = CreateInventoryCommand();
            aiSpec.InventoryParameterSpec[0].AntennaConfiguration[0].AirProtocolInventoryCommandSettings.Add(c1g2Inv);
            msg.ROSpec.SpecParameter.Add(aiSpec);

            MSG_ADD_ROSPEC_RESPONSE rsp = reader.ADD_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    return;
                }
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                return;
            }
            else
            {
                Console.WriteLine("ADD_ROSPEC Command Timed out\n");
                reader.Close();
                return;
            }
        }

        public void EnableRoSpec(int roSpectID)
        {
            Console.WriteLine("Enabling RoSpec\n");
            MSG_ENABLE_ROSPEC msg = new MSG_ENABLE_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = (uint)roSpectID; // this better match the ROSpec we created above
            MSG_ENABLE_ROSPEC_RESPONSE rsp = reader.ENABLE_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    return;
                }
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                return;
            }
            else
            {
                Console.WriteLine("ENABLE_ROSPEC Command Timed out\n");
                reader.Close();
                return;
            }
        }
        public void StartRoSpec(int roSpectID)
        {
            Console.WriteLine("Starting RoSpec\n");
            MSG_START_ROSPEC msg = new MSG_START_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = (uint)roSpectID; // this better match the RoSpec we created above
            MSG_START_ROSPEC_RESPONSE rsp = reader.START_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    return;
                }
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                return;
            }
            else
            {
                Console.WriteLine("START_ROSPEC Command Timed out\n");
                reader.Close();
                return;
            }
        }
        public void DeleteRoSpec(int roSpectID)
        {
            Console.WriteLine("Stopping RoSpec\n");
            MSG_DELETE_ROSPEC msg = new MSG_DELETE_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = (uint)roSpectID; // this better match the RoSpec we created above
            MSG_DELETE_ROSPEC_RESPONSE rsp = reader.DELETE_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    return;
                }
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                return;
            }
            else
            {
                Console.WriteLine("STOP_ROSPEC Command Timed out\n");
                reader.Close();
                return;
            }
        }
        public void StopRoSpec(int roSpectID)
        {
            Console.WriteLine("Stopping RoSpec\n");
            MSG_STOP_ROSPEC msg = new MSG_STOP_ROSPEC();
            MSG_ERROR_MESSAGE msg_err;
            msg.ROSpecID = (uint)roSpectID; // this better match the RoSpec we created above
            MSG_STOP_ROSPEC_RESPONSE rsp = reader.STOP_ROSPEC(msg, out msg_err, 12000);
            if (rsp != null)
            {
                if (rsp.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    return;
                }
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                return;
            }
            else
            {
                Console.WriteLine("STOP_ROSPEC Command Timed out\n");
                reader.Close();
                return;
            }
        }
        public void CleanUpReaderConfiguration()
        {
            Console.WriteLine("Factory Default the Reader\n");

            // factory default the reader
            MSG_SET_READER_CONFIG msg_cfg = new MSG_SET_READER_CONFIG();
            MSG_ERROR_MESSAGE msg_err;

            msg_cfg.ResetToFactoryDefault = true;
            msg_cfg.MSG_ID = 2; // not this doesn't need to bet set as the library will default

            // Note that if SET_READER_CONFIG affects antennas it could take several seconds to return
            MSG_SET_READER_CONFIG_RESPONSE rsp_cfg = reader.SET_READER_CONFIG(msg_cfg, out msg_err, 12000);

            if (rsp_cfg != null)
            {
                if (rsp_cfg.LLRPStatus.StatusCode != ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine(rsp_cfg.LLRPStatus.StatusCode.ToString());
                    reader.Close();
                    return;
                }
            }
            else if (msg_err != null)
            {
                Console.WriteLine(msg_err.ToString());
                reader.Close();
                return;
            }
            else
            {
                Console.WriteLine("SET_READER_CONFIG Command Timed out\n");
                reader.Close();
                return;
            }
        }
        public void Close()
        {

            Console.WriteLine("  Received " + reportCount + " Tag Reports.");
            Console.WriteLine("  Received " + eventCount + " Events.");
            Console.WriteLine("Closing\n");
            // clean up the reader
            reader.Close();
            reader.OnReaderEventNotification -= new delegateReaderEventNotification(reader_OnReaderEventNotification);
            reader.OnRoAccessReportReceived -= new delegateRoAccessReport(reader_OnRoAccessReportReceived);
        }
        public static PARAM_C1G2InventoryCommand CreateInventoryCommand(int Q)
        {
            // Create the Inventory Command and RF Control parameters
            PARAM_C1G2InventoryCommand c1g2Inv = new PARAM_C1G2InventoryCommand();




            // Set the session.
            PARAM_C1G2SingulationControl c1g2Sing = new PARAM_C1G2SingulationControl();
            c1g2Sing.Session = new TwoBits(1);
            c1g2Sing.TagPopulation = (ushort)Math.Pow(2, Q);
            c1g2Sing.TagTransitTime = 0;
            c1g2Inv.C1G2SingulationControl = c1g2Sing;
            c1g2Inv.TagInventoryStateAware = false;

            // Set the search mode.
            PARAM_ImpinjInventorySearchMode impISM = new PARAM_ImpinjInventorySearchMode();
            impISM.InventorySearchMode = ENUM_ImpinjInventorySearchType.Dual_Target;
            c1g2Inv.Custom.Add(impISM);
            return c1g2Inv;
        }
        public static PARAM_C1G2InventoryCommand CreateInventoryCommand()
        {
            // Create the Inventory Command and RF Control parameters
            PARAM_C1G2InventoryCommand c1g2Inv = new PARAM_C1G2InventoryCommand();




            // Set the session.
            PARAM_C1G2SingulationControl c1g2Sing = new PARAM_C1G2SingulationControl();
            c1g2Sing.Session = new TwoBits(1);
            c1g2Sing.TagTransitTime = 0;
            c1g2Sing.TagPopulation = 11;
            c1g2Inv.C1G2SingulationControl = c1g2Sing;
            c1g2Inv.TagInventoryStateAware = false;


            // Set the search mode.
            PARAM_ImpinjInventorySearchMode impISM = new PARAM_ImpinjInventorySearchMode();
            impISM.InventorySearchMode = ENUM_ImpinjInventorySearchType.Dual_Target;
            c1g2Inv.Custom.Add(impISM);
            return c1g2Inv;
        }
        public static PARAM_C1G2InventoryCommand CreateInventoryCommand(ushort pos, String mask, int truncate, int MB)
        {
            // Create the Inventory Command and RF Control parameters
            PARAM_C1G2InventoryCommand c1g2Inv = new PARAM_C1G2InventoryCommand();


            // Setup the tag filter
            c1g2Inv.C1G2Filter = new PARAM_C1G2Filter[1];

            c1g2Inv.C1G2Filter[0] = new PARAM_C1G2Filter();
            c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask();
            // Filter on EPC (memory bank #1)
            c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask.MB = new TwoBits((ushort)MB);
            // Start filtering at the address 0x20 (the start of the third word).
            // The first two words of the EPC are the checksum and Protocol Control bits.
            //E20030093116008216007210
            //c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask.Pointer = (ushort)(0x20 + 4 * pos);

            //c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask.Pointer = (ushort)(0x00 + 4 * pos);
            //c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask.TagMask = LLRPBitArray.FromHexString(mask);

            c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask.Pointer = (ushort)(0x20 + pos);
            c1g2Inv.C1G2Filter[0].C1G2TagInventoryMask.TagMask = LLRPBitArray.FromBinString(mask);
            c1g2Inv.C1G2Filter[0].T = (ENUM_C1G2TruncateAction)truncate;
            c1g2Inv.TagInventoryStateAware = false;
            c1g2Inv.C1G2Filter[0].C1G2TagInventoryStateAwareFilterAction = new PARAM_C1G2TagInventoryStateAwareFilterAction();
            c1g2Inv.C1G2Filter[0].C1G2TagInventoryStateAwareFilterAction.Action = ENUM_C1G2StateAwareAction.DeassertSLOrB_AssertSLOrA;
            c1g2Inv.C1G2Filter[0].C1G2TagInventoryStateAwareFilterAction.Target = ENUM_C1G2StateAwareTarget.SL;



            /* c1g2Inv.C1G2Filter[1] = new PARAM_C1G2Filter();
             c1g2Inv.C1G2Filter[1].C1G2TagInventoryMask = new PARAM_C1G2TagInventoryMask();
             // Filter on EPC (memory bank #1)
             c1g2Inv.C1G2Filter[1].C1G2TagInventoryMask.MB = new TwoBits(1);
             // Start filtering at the address 0x20 (the start of the third word).
             // The first two words of the EPC are the checksum and Protocol Control bits.
             //E20030093116008216007210
             c1g2Inv.C1G2Filter[1].C1G2TagInventoryMask.Pointer = (ushort)(0x20 + 4 * 0);
             c1g2Inv.C1G2Filter[1].C1G2TagInventoryMask.TagMask = LLRPBitArray.FromHexString("D");
             c1g2Inv.C1G2Filter[1].T = (ENUM_C1G2TruncateAction)truncate;
             c1g2Inv.TagInventoryStateAware = true;
             //c1g2Inv.C1G2Filter[1].C1G2TagInventoryStateAwareFilterAction = new PARAM_C1G2TagInventoryStateAwareFilterAction();
             //c1g2Inv.C1G2Filter[1].C1G2TagInventoryStateAwareFilterAction.Action = ENUM_C1G2StateAwareAction.DeassertSLOrB_AssertSLOrA;
             //c1g2Inv.C1G2Filter[1].C1G2TagInventoryStateAwareFilterAction.Target = ENUM_C1G2StateAwareTarget.SL;
             */
            // Set the session.
            PARAM_C1G2SingulationControl c1g2Sing = new PARAM_C1G2SingulationControl();
            c1g2Sing.Session = new TwoBits(1);
            c1g2Sing.TagPopulation = 64;
            c1g2Sing.TagTransitTime = 0;
            c1g2Sing.C1G2TagInventoryStateAwareSingulationAction = new PARAM_C1G2TagInventoryStateAwareSingulationAction();
            //c1g2Sing.C1G2TagInventoryStateAwareSingulationAction.I = ENUM_C1G2TagInventoryStateAwareI.State_A;
            //c1g2Sing.C1G2TagInventoryStateAwareSingulationAction.S = ENUM_C1G2TagInventoryStateAwareS.SL;

            c1g2Inv.C1G2SingulationControl = c1g2Sing;


            // Set the search mode.
            PARAM_ImpinjInventorySearchMode impISM = new PARAM_ImpinjInventorySearchMode();

            impISM.InventorySearchMode = ENUM_ImpinjInventorySearchType.Dual_Target;
            c1g2Inv.Custom.Add(impISM);
            return c1g2Inv;
        }

        public void Add_AccessSpec()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_ADD_ACCESSSPEC msg = new MSG_ADD_ACCESSSPEC();
            msg.AccessSpec = new PARAM_AccessSpec();

            /////////////////////////////////////////////////
            // AccessSpec
            /////////////////////////////////////////////////
            // AccessSpecID should be set to a unique identifier.
            msg.AccessSpec.AccessSpecID = 456;
            msg.AccessSpec.AntennaID = 0;
            // We're writing to a Gen2 tag
            msg.AccessSpec.ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2;
            // AccessSpecs must be disabled when you add them.
            msg.AccessSpec.CurrentState = ENUM_AccessSpecState.Disabled;
            msg.AccessSpec.ROSpecID = 0;
            // Setup the triggers
            msg.AccessSpec.AccessSpecStopTrigger =
                new PARAM_AccessSpecStopTrigger();
            msg.AccessSpec.AccessSpecStopTrigger.AccessSpecStopTrigger =
                ENUM_AccessSpecStopTriggerType.Null;
            // OperationCountValue indicate the number of times this Spec is
            // executed before it is deleted. If set to 0, this is equivalent
            // to no stop trigger defined.
            msg.AccessSpec.AccessSpecStopTrigger.OperationCountValue = 0;

            /////////////////////////////////////////////////
            // AccessCommand
            //
            // Define which tags we want to write to.
            /////////////////////////////////////////////////
            msg.AccessSpec.AccessCommand = new PARAM_AccessCommand();
            msg.AccessSpec.AccessCommand.AirProtocolTagSpec =
                new UNION_AirProtocolTagSpec();
            PARAM_C1G2TagSpec tagSpec = new PARAM_C1G2TagSpec();
            // Specify the target tag. Which tag do we want to write to?
            tagSpec.C1G2TargetTag = new PARAM_C1G2TargetTag[1];
            tagSpec.C1G2TargetTag[0] = new PARAM_C1G2TargetTag();
            tagSpec.C1G2TargetTag[0].Match = true;
            // We'll use the tag's EPC to determine if this is the label we want.
            // Set the memory bank to 1 (The EPC memory bank on a Monza 4 tag).
            tagSpec.C1G2TargetTag[0].MB = new TwoBits(1);
            // The first (msb) bit location of the specified memory
            // bank against which to compare the TagMask.
            // We'll set it to 0x20, to skip the protocol
            // control bits and CRC.
            tagSpec.C1G2TargetTag[0].Pointer = 0x20;
            tagSpec.C1G2TargetTag[0].TagMask =
                LLRPBitArray.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFF");
            tagSpec.C1G2TargetTag[0].TagData =
                LLRPBitArray.FromHexString("300833B2DDD9014000000000");
            msg.AccessSpec.AccessCommand.AirProtocolTagSpec.Add(tagSpec);

            /////////////////////////////////////////////////
            // AccessCommandOpSpec
            //
            // Define the data we want to write.
            /////////////////////////////////////////////////
            msg.AccessSpec.AccessCommand.AccessCommandOpSpec =
                new UNION_AccessCommandOpSpec();
            PARAM_C1G2Write wr = new PARAM_C1G2Write();
            wr.AccessPassword = 0;
            // Bank 3 is user memory on a Monza 4 tag.
            wr.MB = new TwoBits(3);
            // OpSpecID should be set to a unique identifier.
            wr.OpSpecID = 111;
            // Write to the base of user memory.
            wr.WordPointer = 0x00;
            // Data to be written.
            wr.WriteData = UInt16Array.FromHexString("0123456789ABCDEF");
            msg.AccessSpec.AccessCommand.AccessCommandOpSpec.Add(wr);

            /////////////////////////////////////////////////
            // AccessReportSpec
            //
            // Define when we want to receive AccessReports
            /////////////////////////////////////////////////
            msg.AccessSpec.AccessReportSpec = new PARAM_AccessReportSpec();
            msg.AccessSpec.AccessReportSpec.AccessReportTrigger =
                ENUM_AccessReportTriggerType.End_Of_AccessSpec;

            // Send the message and check the reply
            MSG_ADD_ACCESSSPEC_RESPONSE rsp =
                reader.ADD_ACCESSSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                // Success
                Console.WriteLine(rsp.ToString());
            }
            else if (msg_err != null)
            {
                // Error
                Console.WriteLine(msg_err.ToString());
            }
            else
            {
                // Timeout
                Console.WriteLine("Timeout Error.");
            }
        }
        public void Delete_AccessSpec()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_DELETE_ACCESSSPEC msg = new MSG_DELETE_ACCESSSPEC();
            msg.AccessSpecID = 0;
            // Delete all AccessSpecs
            MSG_DELETE_ACCESSSPEC_RESPONSE rsp =
                reader.DELETE_ACCESSSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                // Success
                Console.WriteLine(rsp.ToString());
            }
            else if (msg_err != null)
            {
                // Error
                Console.WriteLine(msg_err.ToString());
            }
            else
            {
                // Timeout
                Console.WriteLine("Timeout Error.");
            }
        }
        public void Enable_AccessSpec()
        {
            MSG_ERROR_MESSAGE msg_err;
            MSG_ENABLE_ACCESSSPEC msg = new MSG_ENABLE_ACCESSSPEC();
            msg.AccessSpecID = 456;
            MSG_ENABLE_ACCESSSPEC_RESPONSE rsp =
                reader.ENABLE_ACCESSSPEC(msg, out msg_err, 2000);
            if (rsp != null)
            {
                // Success
                Console.WriteLine(rsp.ToString());
            }
            else if (msg_err != null)
            {
                // Error
                Console.WriteLine(msg_err.ToString());
            }
            else
            {
                // Timeout
                Console.WriteLine("Timeout Error.");
            }
        }
    }
}
