namespace com.eze.api 
{
public class EzeResult 
{

    private EventName eventName;
	private Status status;
	private string code;
	private string message;
	private PaymentResult paymentResult;
	
	public PaymentResult getPaymentResult() {
		return paymentResult;
	}
	public void setPaymentResult(PaymentResult paymentResult) {
		this.paymentResult = paymentResult;
	}
    public EventName getEventName()
    {
        return eventName;
	}
    public void setEventName(EventName eventName)
    {
        this.eventName = eventName;
	}
	public Status getStatus() {
		return status;
	}
	public void setStatus(Status status) {
		this.status = status;
	}
	public string getCode() {
		return code;
	}
	public void setCode(string code) {
		this.code = code;
	}
	public string getMessage() {
		return message;
	}
	public void setMessage(string message) {
		this.message = message;
	}
	
	public override string ToString() {
        return "EzeResult [eventName=" + eventName + ", status=" + status + ", code=" + code + ", message=" + message
				+ ", paymentResult=" + paymentResult + "]";
	}
}
}