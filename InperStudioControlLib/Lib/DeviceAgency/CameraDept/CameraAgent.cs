namespace InperStudioControlLib.Lib.DeviceAgency.CameraDept
{
    //public class CameraStatusChangedEventArgs : EventArgs
    //{
    //    private string _CameraStatus;
    //    public CameraStatusChangedEventArgs(string status)
    //    {
    //        _CameraStatus = status;
    //    }
    //    public string CameraStatus => _CameraStatus;
    //}


    //class CameraAgent
    //{
    //    public event EventHandler<CameraStatusChangedEventArgs> CameraStatusChanged;

    //    public void RaiseCameraStatusChangedEvent(string status)
    //    {
    //        CameraStatusChanged?.Invoke(this, new CameraStatusChangedEventArgs(status));
    //    }


    //    private BaslerCamera cam = null;

    //    public static CameraAgent Instance { get; } = new CameraAgent();

    //    private IDeviceNotifier USBDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
    //    private CameraAgent()
    //    {
    //        USBDeviceNotifier.OnDeviceNotify += new EventHandler<DeviceNotifyEventArgs>(OnDeviceNotifyEvent);
    //    }


    //    private int _DeviceVID = 0x2676;
    //    private int _DevicePID = 0xBA02;
    //    private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
    //    {
    //        if (e.DeviceType == DeviceType.DeviceInterface &&
    //            e.Device.IdVendor == _DeviceVID &&
    //            e.Device.IdProduct == _DevicePID)
    //        {
    //            if (e.EventType == EventType.DeviceArrival)
    //            {
    //                if (cam == null)
    //                {
    //                    try
    //                    {
    //                        cam = new BaslerCamera();
    //                        if (cam == null)
    //                        {
    //                            HandyControl.Controls.Growl.Error("OnDeviceNotifyEvent: device not found.");
    //                            return;
    //                        }
    //                        else
    //                        {
    //                            cam.Open();
    //                        }
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        System.Diagnostics.Debug.WriteLine(ex.Message);
    //                    }

    //                }
    //                else
    //                {
    //                    cam.Reset();
    //                }
    //                RaiseCameraStatusChangedEvent("CameraConnected");
    //            }
    //            else if (e.EventType == EventType.DeviceRemoveComplete)
    //            {
    //                RaiseCameraStatusChangedEvent("CameraDisconnected");
    //            }
    //        }

    //        return;
    //    }


    //    public BaslerCamera CreateCamera()
    //    {
    //        if (cam == null)
    //        {
    //            try
    //            {
    //                cam = new BaslerCamera();
    //                if (cam == null)
    //                {
    //                    HandyControl.Controls.Growl.Error("OnDeviceNotifyEvent: device not found.");
    //                }
    //                else
    //                {
    //                    cam.Open();
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                System.Diagnostics.Debug.WriteLine(e.Message);
    //            }
    //        }

    //        return cam;
    //    }
    //}
}
