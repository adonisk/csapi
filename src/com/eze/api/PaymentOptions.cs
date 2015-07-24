using System;
using System.Collections.Generic;
using System.Collections;

namespace com.eze.api {
public class PaymentOptions {
    string orderId;
	string receiptType;
	string chequeNo;
	string bankCode;
	String chequeDate;
	
	public static PaymentOptions build() {
		return new PaymentOptions();
	}

	public string getOrderId() {
		return orderId;
	}
	public PaymentOptions setOrderId(string orderId) {
		this.orderId = orderId;
		return this;
	}
	public string getReceiptType() {
		return receiptType;
	}
	public PaymentOptions setReceiptType(string receiptType) {
		this.receiptType = receiptType;
		return this;
	}
	public string getChequeNo() {
		return chequeNo;
	}
	public PaymentOptions setChequeNo(string chequeNo) {
		this.chequeNo = chequeNo;
		return this;
	}
	public string getBankCode() {
		return bankCode;
	}
	public PaymentOptions setBankCode(string bankCode) {
		this.bankCode = bankCode;
		return this;
	}
	public String getChequeDate() {
		return chequeDate;
	}
	//public string getChequeDateAsString() {
		//SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd");
		//return df.format(this.chequeDate);
        //return this.chequeDate.ToString("yyyy-MM-dd");
	//}
	public PaymentOptions setChequeDate(String chequeDate) {
		this.chequeDate = chequeDate;
		return this;
	}
	
	public string toString() {
		return "orderId=" + this.orderId 
				+ ", receiptType=" + this.receiptType 
				+ ", chequeNo=" + this.chequeNo
				+ ", bankName=" + this.bankCode 
				+ ", chequeDate=" + this.chequeDate;
	}
}
}