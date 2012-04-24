using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PortableDeviceApiLib;

namespace PortableDeviceLib
{
  public class StillImageDevice:PortableDevice
  {
    public StillImageDevice(string deviceId)
      :base(deviceId)
    {
   
    }

    public void ExecuteWithNoData(int code)
    {
      IPortableDeviceValues commandValues = (IPortableDeviceValues)new PortableDeviceTypesLib.PortableDeviceValuesClass();
      IPortableDevicePropVariantCollection propVariant =
        (IPortableDevicePropVariantCollection)new PortableDeviceTypesLib.PortableDevicePropVariantCollection();
      IPortableDeviceValues results;

      //commandValues.SetGuidValue(ref PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY, ref command.fmtid);
      commandValues.SetGuidValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY,
                                       PortableDevicePKeys.WPD_COMMAND_MTP_EXT_EXECUTE_COMMAND_WITHOUT_DATA_PHASE.fmtid);
      commandValues.SetUnsignedIntegerValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_ID,
                             PortableDevicePKeys.WPD_COMMAND_MTP_EXT_EXECUTE_COMMAND_WITHOUT_DATA_PHASE.pid);

      commandValues.SetIPortableDevicePropVariantCollectionValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_PARAMS, propVariant);
      commandValues.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_CODE, (uint) code );

      // According to documentation, first parameter should be 0 (see http://msdn.microsoft.com/en-us/library/dd375691%28v=VS.85%29.aspx)
      this.portableDeviceClass.SendCommand(0, commandValues, out results);
      int pvalue = 0;
      try
      {
        int pValue = 0;
        results.GetErrorValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_HRESULT, out pValue);
        if (pValue != 0)
          return ;
      }
      catch (Exception ex)
      {
      }
    }

    public void ExecuteWithNoData(int code, uint param1, uint param2)
    {
      IPortableDeviceValues commandValues =
        (IPortableDeviceValues) new PortableDeviceTypesLib.PortableDeviceValuesClass();
      IPortableDevicePropVariantCollection propVariant =
        (IPortableDevicePropVariantCollection) new PortableDeviceTypesLib.PortableDevicePropVariantCollection();
      IPortableDeviceValues results;

      //commandValues.SetGuidValue(ref PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY, ref command.fmtid);
      commandValues.SetGuidValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY,
                                 PortableDevicePKeys.WPD_COMMAND_MTP_EXT_EXECUTE_COMMAND_WITHOUT_DATA_PHASE.fmtid);
      commandValues.SetUnsignedIntegerValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_ID,
                                            PortableDevicePKeys.WPD_COMMAND_MTP_EXT_EXECUTE_COMMAND_WITHOUT_DATA_PHASE.
                                              pid);

      tag_inner_PROPVARIANT vparam1 = new tag_inner_PROPVARIANT();
      tag_inner_PROPVARIANT vparam2 = new tag_inner_PROPVARIANT();
      UintToPropVariant(param1, out vparam1);
      propVariant.Add(ref vparam1);
      UintToPropVariant(param2, out vparam2);
      propVariant.Add(ref vparam2);
      
      commandValues.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_CODE, (uint)code);
      
      commandValues.SetIPortableDevicePropVariantCollectionValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_PARAMS, propVariant);

      // According to documentation, first parameter should be 0 (see http://msdn.microsoft.com/en-us/library/dd375691%28v=VS.85%29.aspx)
      this.portableDeviceClass.SendCommand(0, commandValues, out results);
      int pvalue = 0;
      try
      {
        int pValue = 0;
        results.GetErrorValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_HRESULT, out pValue);
        if (pValue != 0)
          return;
      }
      catch (Exception ex)
      {
      }
      //results.GetSignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_RESPONSE_CODE, out pvalue);
    }

    public byte[] ExecuteWithData(int code)
    {
      return ExecuteWithData(code, -1, -1);
    }

    public byte[] ExecuteWithData(int code, int param1, int param2)
    {
      // source: http://msdn.microsoft.com/en-us/library/windows/desktop/ff384843(v=vs.85).aspx
      // and view-source:http://www.experts-exchange.com/Programming/Languages/C_Sharp/Q_26860397.html
      // error codes http://msdn.microsoft.com/en-us/library/windows/desktop/dd319335(v=vs.85).aspx
      byte[] imgdate = new byte[8];

      IPortableDeviceValues commandValues = (IPortableDeviceValues)new PortableDeviceTypesLib.PortableDeviceValuesClass();
      IPortableDeviceValues pParameters = (IPortableDeviceValues)new PortableDeviceTypesLib.PortableDeviceValues();

      IPortableDevicePropVariantCollection propVariant =
        (IPortableDevicePropVariantCollection)new PortableDeviceTypesLib.PortableDevicePropVariantCollection();
      IPortableDeviceValues pResults;

      //commandValues.SetGuidValue(ref PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY, ref command.fmtid);
      commandValues.SetGuidValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY,
                                       PortableDevicePKeys.WPD_COMMAND_MTP_EXT_EXECUTE_COMMAND_WITH_DATA_TO_READ.fmtid);
      commandValues.SetUnsignedIntegerValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_ID,
                             PortableDevicePKeys.WPD_COMMAND_MTP_EXT_EXECUTE_COMMAND_WITH_DATA_TO_READ.pid);
      commandValues.SetBufferValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_DATA, ref imgdate[0], (uint)imgdate.Length);


      tag_inner_PROPVARIANT vparam1 = new tag_inner_PROPVARIANT();
      tag_inner_PROPVARIANT vparam2 = new tag_inner_PROPVARIANT();
      if (param1 > -1)
      {
        UintToPropVariant((uint)param1, out vparam1);
        propVariant.Add(ref vparam1);
      }
      if (param2 > -1)
      {
        UintToPropVariant((uint)param2, out vparam2);
        propVariant.Add(ref vparam2);
      }
      commandValues.SetIPortableDevicePropVariantCollectionValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_PARAMS, propVariant);
      commandValues.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_CODE, (uint) code);

      // According to documentation, first parameter should be 0 (see http://msdn.microsoft.com/en-us/library/dd375691%28v=VS.85%29.aspx)
      this.portableDeviceClass.SendCommand(0, commandValues, out pResults);

      try
      {
        int pValue = 0;
        pResults.GetErrorValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_HRESULT, out pValue);
        if (pValue != 0)
        {
          return null;
        }
      }
      catch (Exception ex)
      {
      }
      string pwszContext = string.Empty;
      pResults.GetStringValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_CONTEXT, out pwszContext);
      uint cbReportedDataSize = 0;
      pResults.GetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_TOTAL_DATA_SIZE, out cbReportedDataSize);


      uint tmpBufferSize = 0;
      uint tmpTransferSize = 0;
      string tmpTransferContext = string.Empty;
      {
        pResults.GetStringValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_CONTEXT, out tmpTransferContext);
        pResults.GetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_TOTAL_DATA_SIZE, out tmpBufferSize);
        pResults.GetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPTIMAL_TRANSFER_BUFFER_SIZE, out tmpTransferSize);

        try
        {
          int pValue;
          pResults.GetErrorValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_HRESULT, out pValue);
          if (pValue != 0)
          {
            return null;
          }
        }
        catch
        {
        }
      }

      pParameters.Clear();
      pResults.Clear();

      byte[] tmpData = new byte[(int)tmpTransferSize];
      //CCustomReadContext{81CD75F1-A997-4DA2-BAB1-FF5EC514E355}
      pParameters.SetGuidValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY, PortableDevicePKeys.WPD_COMMAND_MTP_EXT_READ_DATA.fmtid);
      pParameters.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_ID, PortableDevicePKeys.WPD_COMMAND_MTP_EXT_READ_DATA.pid);
      pParameters.SetStringValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_CONTEXT, tmpTransferContext);
      pParameters.SetBufferValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_DATA, ref tmpData[0], (uint)tmpTransferSize);
      pParameters.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_NUM_BYTES_TO_READ, (uint)tmpTransferSize);
      pParameters.SetIPortableDevicePropVariantCollectionValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_OPERATION_PARAMS, propVariant);


      portableDeviceClass.SendCommand(0, pParameters, out pResults);


      uint cbBytesRead = 0;

      try
      {
        int pValue = 0;
        pResults.GetErrorValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_HRESULT, out pValue);
        if (pValue != 0)
          return null;
      }
      catch (Exception ex)
      {
      }
      // 24,142,174,9
      // 18, 8E  
      GCHandle pinnedArray = GCHandle.Alloc(imgdate, GCHandleType.Pinned);
      IntPtr ptr = pinnedArray.AddrOfPinnedObject();

      uint dataread = 0;
      pResults.GetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_NUM_BYTES_READ, out dataread);
      pResults.GetBufferValue(ref PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_DATA, ptr, out cbBytesRead);

      IntPtr tmpPtr = new IntPtr(Marshal.ReadInt64(ptr));
      byte[] res = new byte[(int)cbBytesRead];
      for (int i = 0; i < cbBytesRead; i++)
      {
        res[i] = Marshal.ReadByte(tmpPtr, i);
      }

      pParameters.Clear();
      pResults.Clear();
      {
        pParameters.SetGuidValue(PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_CATEGORY, PortableDevicePKeys.WPD_COMMAND_MTP_EXT_END_DATA_TRANSFER.fmtid);
        pParameters.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_PROPERTY_COMMON_COMMAND_ID, PortableDevicePKeys.WPD_COMMAND_MTP_EXT_END_DATA_TRANSFER.pid);
        pParameters.SetStringValue(PortableDevicePKeys.WPD_PROPERTY_MTP_EXT_TRANSFER_CONTEXT, tmpTransferContext);
      }

      portableDeviceClass.SendCommand(0, pParameters, out pResults);

      Marshal.FreeHGlobal(tmpPtr);
      pinnedArray.Free();
      //Marshal.FreeHGlobal(ptr);

      try
      {
        int tmpResult = 0;

        pResults.GetErrorValue(ref PortableDevicePKeys.WPD_PROPERTY_COMMON_HRESULT, out tmpResult);
        if (tmpResult != 0)
        {

        }
      }
      catch
      {
      }
      return res;
    }

    private void StringToPropVariant(string value,
        out PortableDeviceApiLib.tag_inner_PROPVARIANT propvarValue)
    {
      // We'll use an IPortableDeviceValues object to transform the
      // string into a PROPVARIANT
      PortableDeviceApiLib.IPortableDeviceValues pValues =
          (PortableDeviceApiLib.IPortableDeviceValues)
              new PortableDeviceTypesLib.PortableDeviceValuesClass();

      // We insert the string value into the IPortableDeviceValues object
      // using the SetStringValue method
      pValues.SetStringValue(ref PortableDevicePKeys.WPD_OBJECT_ID, value);

      // We then extract the string into a PROPVARIANT by using the 
      // GetValue method
      pValues.GetValue(ref PortableDevicePKeys.WPD_OBJECT_ID,
                                  out propvarValue);
    }

    private void UintToPropVariant(uint value,
    out PortableDeviceApiLib.tag_inner_PROPVARIANT propvarValue)
    {
      // We'll use an IPortableDeviceValues object to transform the
      // string into a PROPVARIANT
      PortableDeviceApiLib.IPortableDeviceValues pValues =
          (PortableDeviceApiLib.IPortableDeviceValues)
              new PortableDeviceTypesLib.PortableDeviceValuesClass();

      // We insert the string value into the IPortableDeviceValues object
      // using the SetStringValue method
      pValues.SetUnsignedIntegerValue(ref PortableDevicePKeys.WPD_OBJECT_ID, (uint)value);

      // We then extract the string into a PROPVARIANT by using the 
      // GetValue method
      pValues.GetValue(ref PortableDevicePKeys.WPD_OBJECT_ID,
                                  out propvarValue);
    }

    private void uLongIntToPropVariant(ulong value, out PortableDeviceApiLib.tag_inner_PROPVARIANT propvarValue)
    {
      // We'll use an IPortableDeviceValues object to transform the
      // string into a PROPVARIANT
      PortableDeviceApiLib.IPortableDeviceValues pValues =
          (PortableDeviceApiLib.IPortableDeviceValues)
              new PortableDeviceTypesLib.PortableDeviceValuesClass();

      // We insert the string value into the IPortableDeviceValues object
      // using the SetStringValue method
      pValues.SetUnsignedLargeIntegerValue(ref PortableDevicePKeys.WPD_OBJECT_ID, (ulong)value);

      // We then extract the string into a PROPVARIANT by using the 
      // GetValue method
      pValues.GetValue(ref PortableDevicePKeys.WPD_OBJECT_ID,
                                  out propvarValue);
    }
  }
}
