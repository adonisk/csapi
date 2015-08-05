
namespace com.eze.api 
{
    public enum Status 
    {
        SUCCESS, FAILURE
    }
    public enum PaymentType
    {
        CARD, CASH, CHEQUE
    }

    public enum LoginMode
    {
        PASSWORD, APPKEY
    }

    public enum ServerType
    {
        PROD, DEMO
    }

    public enum EventName
    {
        LOGIN, LOGOUT, EXIT, PREPARE_DEVICE, NOTIFICATION, TAKE_PAYMENT, CREATE, SET_SERVER_TYPE, SEND_RECEIPT, OTHER
    }
}