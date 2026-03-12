using Org.LLRP.LTK.LLRPV1.DataType;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class LLRPXmlParser
  {
    public static void ParseXMLToLLRPMessage(
      string xmlstr,
      out Message msg,
      out ENUM_LLRP_MSG_TYPE type)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(xmlstr);
      XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
      string name = documentElement.Name;
      if (name != null)
      {
        switch (name.Length)
        {
          case 9:
            if (name == "KEEPALIVE")
            {
              msg = (Message) MSG_KEEPALIVE.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.KEEPALIVE;
              return;
            }
            break;
          case 10:
            switch (name[0])
            {
              case 'A':
                if (name == "ADD_ROSPEC")
                {
                  msg = (Message) MSG_ADD_ROSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ADD_ROSPEC;
                  return;
                }
                break;
              case 'G':
                if (name == "GET_REPORT")
                {
                  msg = (Message) MSG_GET_REPORT.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.GET_REPORT;
                  return;
                }
                break;
            }
            break;
          case 11:
            switch (name[0])
            {
              case 'G':
                if (name == "GET_ROSPECS")
                {
                  msg = (Message) MSG_GET_ROSPECS.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.GET_ROSPECS;
                  return;
                }
                break;
              case 'S':
                if (name == "STOP_ROSPEC")
                {
                  msg = (Message) MSG_STOP_ROSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.STOP_ROSPEC;
                  return;
                }
                break;
            }
            break;
          case 12:
            if (name == "START_ROSPEC")
            {
              msg = (Message) MSG_START_ROSPEC.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.START_ROSPEC;
              return;
            }
            break;
          case 13:
            switch (name[2])
            {
              case 'A':
                if (name == "ENABLE_ROSPEC")
                {
                  msg = (Message) MSG_ENABLE_ROSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ENABLE_ROSPEC;
                  return;
                }
                break;
              case 'E':
                if (name == "KEEPALIVE_ACK")
                {
                  msg = (Message) MSG_KEEPALIVE_ACK.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.KEEPALIVE_ACK;
                  return;
                }
                break;
              case 'L':
                if (name == "DELETE_ROSPEC")
                {
                  msg = (Message) MSG_DELETE_ROSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.DELETE_ROSPEC;
                  return;
                }
                break;
              case 'R':
                if (name == "ERROR_MESSAGE")
                {
                  msg = (Message) MSG_ERROR_MESSAGE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ERROR_MESSAGE;
                  return;
                }
                break;
            }
            break;
          case 14:
            switch (name[0])
            {
              case 'A':
                if (name == "ADD_ACCESSSPEC")
                {
                  msg = (Message) MSG_ADD_ACCESSSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ADD_ACCESSSPEC;
                  return;
                }
                break;
              case 'C':
                if (name == "CUSTOM_MESSAGE")
                {
                  msg = (Message) MSG_CUSTOM_MESSAGE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.CUSTOM_MESSAGE;
                  return;
                }
                break;
              case 'D':
                if (name == "DISABLE_ROSPEC")
                {
                  msg = (Message) MSG_DISABLE_ROSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.DISABLE_ROSPEC;
                  return;
                }
                break;
            }
            break;
          case 15:
            if (name == "GET_ACCESSSPECS")
            {
              msg = (Message) MSG_GET_ACCESSSPECS.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.GET_ACCESSSPECS;
              return;
            }
            break;
          case 16:
            switch (name[0])
            {
              case 'C':
                if (name == "CLOSE_CONNECTION")
                {
                  msg = (Message) MSG_CLOSE_CONNECTION.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.CLOSE_CONNECTION;
                  return;
                }
                break;
              case 'R':
                if (name == "RO_ACCESS_REPORT")
                {
                  msg = (Message) MSG_RO_ACCESS_REPORT.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.RO_ACCESS_REPORT;
                  return;
                }
                break;
            }
            break;
          case 17:
            switch (name[0])
            {
              case 'C':
                if (name == "CLIENT_REQUEST_OP")
                {
                  msg = (Message) MSG_CLIENT_REQUEST_OP.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.CLIENT_REQUEST_OP;
                  return;
                }
                break;
              case 'D':
                if (name == "DELETE_ACCESSSPEC")
                {
                  msg = (Message) MSG_DELETE_ACCESSSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.DELETE_ACCESSSPEC;
                  return;
                }
                break;
              case 'E':
                if (name == "ENABLE_ACCESSSPEC")
                {
                  msg = (Message) MSG_ENABLE_ACCESSSPEC.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ENABLE_ACCESSSPEC;
                  return;
                }
                break;
              case 'G':
                if (name == "GET_READER_CONFIG")
                {
                  msg = (Message) MSG_GET_READER_CONFIG.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.GET_READER_CONFIG;
                  return;
                }
                break;
              case 'S':
                if (name == "SET_READER_CONFIG")
                {
                  msg = (Message) MSG_SET_READER_CONFIG.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.SET_READER_CONFIG;
                  return;
                }
                break;
            }
            break;
          case 18:
            if (name == "DISABLE_ACCESSSPEC")
            {
              msg = (Message) MSG_DISABLE_ACCESSSPEC.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.DISABLE_ACCESSSPEC;
              return;
            }
            break;
          case 19:
            if (name == "ADD_ROSPEC_RESPONSE")
            {
              msg = (Message) MSG_ADD_ROSPEC_RESPONSE.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.ADD_ROSPEC_RESPONSE;
              return;
            }
            break;
          case 20:
            switch (name[0])
            {
              case 'G':
                if (name == "GET_ROSPECS_RESPONSE")
                {
                  msg = (Message) MSG_GET_ROSPECS_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.GET_ROSPECS_RESPONSE;
                  return;
                }
                break;
              case 'S':
                if (name == "STOP_ROSPEC_RESPONSE")
                {
                  msg = (Message) MSG_STOP_ROSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.STOP_ROSPEC_RESPONSE;
                  return;
                }
                break;
            }
            break;
          case 21:
            if (name == "START_ROSPEC_RESPONSE")
            {
              msg = (Message) MSG_START_ROSPEC_RESPONSE.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.START_ROSPEC_RESPONSE;
              return;
            }
            break;
          case 22:
            switch (name[0])
            {
              case 'D':
                if (name == "DELETE_ROSPEC_RESPONSE")
                {
                  msg = (Message) MSG_DELETE_ROSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.DELETE_ROSPEC_RESPONSE;
                  return;
                }
                break;
              case 'E':
                if (name == "ENABLE_ROSPEC_RESPONSE")
                {
                  msg = (Message) MSG_ENABLE_ROSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ENABLE_ROSPEC_RESPONSE;
                  return;
                }
                break;
            }
            break;
          case 23:
            switch (name[0])
            {
              case 'A':
                if (name == "ADD_ACCESSSPEC_RESPONSE")
                {
                  msg = (Message) MSG_ADD_ACCESSSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ADD_ACCESSSPEC_RESPONSE;
                  return;
                }
                break;
              case 'D':
                if (name == "DISABLE_ROSPEC_RESPONSE")
                {
                  msg = (Message) MSG_DISABLE_ROSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.DISABLE_ROSPEC_RESPONSE;
                  return;
                }
                break;
              case 'G':
                if (name == "GET_READER_CAPABILITIES")
                {
                  msg = (Message) MSG_GET_READER_CAPABILITIES.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.GET_READER_CAPABILITIES;
                  return;
                }
                break;
            }
            break;
          case 24:
            if (name == "GET_ACCESSSPECS_RESPONSE")
            {
              msg = (Message) MSG_GET_ACCESSSPECS_RESPONSE.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.GET_ACCESSSPECS_RESPONSE;
              return;
            }
            break;
          case 25:
            switch (name[0])
            {
              case 'C':
                if (name == "CLOSE_CONNECTION_RESPONSE")
                {
                  msg = (Message) MSG_CLOSE_CONNECTION_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.CLOSE_CONNECTION_RESPONSE;
                  return;
                }
                break;
              case 'E':
                if (name == "ENABLE_EVENTS_AND_REPORTS")
                {
                  msg = (Message) MSG_ENABLE_EVENTS_AND_REPORTS.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ENABLE_EVENTS_AND_REPORTS;
                  return;
                }
                break;
              case 'R':
                if (name == "READER_EVENT_NOTIFICATION")
                {
                  msg = (Message) MSG_READER_EVENT_NOTIFICATION.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.READER_EVENT_NOTIFICATION;
                  return;
                }
                break;
            }
            break;
          case 26:
            switch (name[0])
            {
              case 'C':
                if (name == "CLIENT_REQUEST_OP_RESPONSE")
                {
                  msg = (Message) MSG_CLIENT_REQUEST_OP_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.CLIENT_REQUEST_OP_RESPONSE;
                  return;
                }
                break;
              case 'D':
                if (name == "DELETE_ACCESSSPEC_RESPONSE")
                {
                  msg = (Message) MSG_DELETE_ACCESSSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.DELETE_ACCESSSPEC_RESPONSE;
                  return;
                }
                break;
              case 'E':
                if (name == "ENABLE_ACCESSSPEC_RESPONSE")
                {
                  msg = (Message) MSG_ENABLE_ACCESSSPEC_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.ENABLE_ACCESSSPEC_RESPONSE;
                  return;
                }
                break;
              case 'G':
                if (name == "GET_READER_CONFIG_RESPONSE")
                {
                  msg = (Message) MSG_GET_READER_CONFIG_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.GET_READER_CONFIG_RESPONSE;
                  return;
                }
                break;
              case 'S':
                if (name == "SET_READER_CONFIG_RESPONSE")
                {
                  msg = (Message) MSG_SET_READER_CONFIG_RESPONSE.FromString(xmlstr);
                  type = ENUM_LLRP_MSG_TYPE.SET_READER_CONFIG_RESPONSE;
                  return;
                }
                break;
            }
            break;
          case 27:
            if (name == "DISABLE_ACCESSSPEC_RESPONSE")
            {
              msg = (Message) MSG_DISABLE_ACCESSSPEC_RESPONSE.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.DISABLE_ACCESSSPEC_RESPONSE;
              return;
            }
            break;
          case 32:
            if (name == "GET_READER_CAPABILITIES_RESPONSE")
            {
              msg = (Message) MSG_GET_READER_CAPABILITIES_RESPONSE.FromString(xmlstr);
              type = ENUM_LLRP_MSG_TYPE.GET_READER_CAPABILITIES_RESPONSE;
              return;
            }
            break;
        }
      }
      type = (ENUM_LLRP_MSG_TYPE) 0;
      msg = (Message) CustomMsgDecodeFactory.DecodeXmlNodeToCustomMessage(documentElement, xmlstr);
      if (msg == null)
        return;
      type = ENUM_LLRP_MSG_TYPE.CUSTOM_MESSAGE;
    }
  }
}
