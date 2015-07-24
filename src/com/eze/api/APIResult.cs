namespace com.eze.api {
public class APIResult {
    
	private string eventType;
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
	public string getEventType() {
		return eventType;
	}
	public void setEventType(string eventType) {
		this.eventType = eventType;
	}
	public Status getStatus() {
		return status;
	}
	public void setStatus(Status status) {
		this.status = status;
	}
	public void setStatus(string status) {
		if (status == Status.SUCCESS.ToString()) {
			this.status = Status.SUCCESS;
		} else if (status == Status.FAILURE.ToString()) {
			this.status = Status.FAILURE;
		}
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
		return "EzeResult [eventType=" + eventType + ", status=" + status + ", code=" + code + ", message=" + message
				+ ", paymentResult=" + paymentResult + "]";
	}
}
}