namespace com.eze.api {
public class PaymentResult {

    private string pmtType;
	private string status;
	private string txnId;
	private double amount;
	private string settlementStatus;
	private bool voidable;
	private string chequeNo;
	private string chequeDate;
	private string authCode;
	private string cardType;
	private string orderId;
	private string tid;
	private string merchantId;

	public string getPmtType() {
		return pmtType;
	}
	public void setPmtType(string pmtType) {
		this.pmtType = pmtType;
	}
	public string getStatus() {
		return status;
	}
	public void setStatus(string status) {
		this.status = status;
	}
	public string getTxnId() {
		return txnId;
	}
	public void setTxnId(string txnId) {
		this.txnId = txnId;
	}
	public double getAmount() {
		return amount;
	}
	public void setAmount(double amount) {
		this.amount = amount;
	}
	public string getSettlementStatus() {
		return settlementStatus;
	}
	public void setSettlementStatus(string settlementStatus) {
		this.settlementStatus = settlementStatus;
	}
	public bool getVoidable() {
		return voidable;
	}
	public void setVoidable(bool voidable) {
		this.voidable = voidable;
	}
	public string getChequeNo() {
		return chequeNo;
	}
	public void setChequeNo(string chequeNo) {
		this.chequeNo = chequeNo;
	}
	public string getChequeDate() {
		return chequeDate;
	}
	public void setChequeDate(string chequeDate) {
		this.chequeDate = chequeDate;
	}
	public string getAuthCode() {
		return authCode;
	}
	public void setAuthCode(string authCode) {
		this.authCode = authCode;
	}
	public string getCardType() {
		return cardType;
	}
	public void setCardType(string cardType) {
		this.cardType = cardType;
	}
	public string getOrderId() {
		return orderId;
	}
	public void setOrderId(string orderId) {
		this.orderId = orderId;
	}
	public string getTid() {
		return tid;
	}
	public void setTid(string tid) {
		this.tid = tid;
	}
	public string getMerchantId() {
		return merchantId;
	}
	public void setMerchantId(string merchantId) {
		this.merchantId = merchantId;
	}
	
	public override string ToString() {
		return "PaymentResult [pmtType=" + pmtType + ", status=" + status + ", txnId=" + txnId + ", amount=" + amount
				+ ", settlementStatus=" + settlementStatus + ", voidable=" + voidable + ", chequeNo=" + chequeNo
				+ ", chequeDate=" + chequeDate + ", authCode=" + authCode + ", cardType=" + cardType + ", orderId="
				+ orderId + ", tid=" + tid + ", merchantId=" + merchantId + "]";
	}
}
}