using System;
using System.IO;
using System.Diagnostics;
using com.eze.exception;
using com.eze.ezecli;
using Google.ProtocolBuffers;

namespace com.eze.api {
public class EzeAPI {

	private Process p;
	private BinaryWriter output;
	private BinaryReader input;
	//private StreamReader err;
	private static EzeAPI API;
	
	private EzeAPI() {
	}
	
	/** 
	 * This method destroys the ezecli exe and gracefully closes the API.
	 */
	public void destroy() {
		if (null != p) {
			try {
				p.Kill();
			} catch (Exception e) {
				Console.Write(e.Message);
			}
		}
	}
	
	/** 
	 * Method returns the Singleton instance of EzeAPI.
	 * 
	 * @return EzeAPI object
	 */
	public static EzeAPI getInstance() {
		
		if (null == API) {
			API = new EzeAPI();
			API.initialize();
		}
		return API;
	}
	
	/**
	 * This method instantiates the Ezecli and setup the input 
	 * and output buffers for reading and writing through protocol buffers.
	 */
	public void initialize() {

		try {
			if (null != p) {
				p.Kill();
			} 

            ProcessStartInfo startInfo = new ProcessStartInfo(getEzecliFile());
            startInfo.CreateNoWindow = true;
            startInfo.ErrorDialog = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            
            p = new Process();
            p.StartInfo = startInfo;
            p.Start();
            input = new BinaryReader(p.StandardOutput.BaseStream);
            output = new BinaryWriter(p.StandardInput.BaseStream);
            //err = p.StandardError;

		} catch (Exception e) {
			Console.Write(e.Message);

			try {
				if (p != null) p.Kill();
			} catch (Exception ex) {
                Console.Write(ex.Message);
		    }
			throw new APIException("Initialize failed. e="+e.Message);
		}
	}

	public APIResult login(string userName, string password) {
		Console.WriteLine("...Login User <"+userName+":"+password+">");
		
		LoginInput loginInput = LoginInput.CreateBuilder()
						.SetLoginMode(LoginInput.Types.LoginMode.PASSWORD)
						.SetUsername(userName)
						.SetPasskey(password).Build();

		ApiInput apiInput = ApiInput.CreateBuilder()
						.SetMsgType(ApiInput.Types.MessageType.LOGIN)
						.SetMsgData(loginInput.ToByteString()).Build();
		
        this.send(apiInput);

        APIResult result = null;
	
		while (true) {
			result = this.getResult(this.receive());
			    if (result.getEventType() != com.eze.ezecli.ApiOutput.Types.EventType.LOGIN_RESULT.ToString()) continue;
			if ((result.getStatus().ToString() == com.eze.ezecli.ApiOutput.Types.ResultStatus.FAILURE.ToString())) {
				throw new APIException("Login failed. "+result.ToString());
			}
			break;
		}

		return result;
	}
	
	public void quit() {
		this.logout().exit();
	}
	
	private EzeAPI logout() {
		Console.WriteLine("...logging out");
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.LOGOUT)
				.Build();

		this.send(apiInput);
		APIResult result = null;
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventType() != ApiOutput.Types.EventType.LOGOUT_RESULT.ToString()) continue;
			if ((result.getStatus().ToString() == ApiOutput.Types.ResultStatus.FAILURE.ToString())) {
				throw new APIException("Logout failed. "+result.ToString());
			}
			break;
		}
		return this;
	}
		
	private EzeAPI exit() {
		Console.WriteLine("...exiting");
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.EXIT)
				.Build();

		this.send(apiInput);
		APIResult result = null;
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventType() != ApiOutput.Types.EventType.EXIT_RESULT.ToString()) continue;
			if ((result.getStatus().ToString() == ApiOutput.Types.ResultStatus.FAILURE.ToString())) {
				throw new APIException("Exit failed. "+result.ToString());
			}
			break;
		}
		return this;
	}
	
	public EzeAPI prepareDevice() {
		Console.WriteLine("...Preparing Device");
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.PREPARE_DEVICE)
				.Build();

		this.send(apiInput);
		APIResult result = null;
		
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventType() != ApiOutput.Types.EventType.PREPARE_DEVICE_RESULT.ToString()) continue;
			if ((result.getStatus().ToString() == ApiOutput.Types.ResultStatus.FAILURE.ToString())) {
				throw new APIException("Prepare device failed. "+result.ToString());
			}
			break;
		}
		return this;
	}
	
	public APIResult takePayment(PaymentType type, double amount, PaymentOptions options) {
		
		APIResult result = null;
		Console.WriteLine("...Take Payment <"+type.ToString()+",amount="+amount+","+">");
		TxnInput.Types.TxnType txnType = TxnInput.Types.TxnType.CARD_PRE_AUTH;
		
        switch(type) {
    		case PaymentType.CARD: {
                txnType = TxnInput.Types.TxnType.CARD_AUTH;
			    break;
            }
		    case PaymentType.CASH: {
			    txnType = TxnInput.Types.TxnType.CASH;
			    break;
            }
		    case PaymentType.CHEQUE: {
			    txnType = TxnInput.Types.TxnType.CHEQUE;
			    break;
            }
		    default: {
			    txnType = TxnInput.Types.TxnType.CARD_PRE_AUTH;
                break;
            }
		}
				
		if (amount <= 0) throw new APIException("Amount is 0 or negative");
		if (txnType == TxnInput.Types.TxnType.CHEQUE) {
			if ((null == options) ||
				(null == options.getChequeNo()) || (options.getChequeNo().Length == 0) ||
				(null == options.getBankCode()) || (options.getBankCode().Length == 0) ||
				(null == options.getChequeDate())) {
				throw new APIException("Cheque details not passed for a Cheque transaction");
			}
		}
		
		TxnInput tInput = TxnInput.CreateBuilder()
				.SetTxnType(txnType)
				.SetAmount(amount)
				.Build();
		
		if (null != options) {
			if (null != options.getOrderId()) tInput = TxnInput.CreateBuilder(tInput).SetOrderId(options.getOrderId()).Build();
			if (null != options.getReceiptType()) tInput = TxnInput.CreateBuilder(tInput).SetReceiptType(options.getReceiptType()).Build();
			if (null != options.getChequeNo()) tInput = TxnInput.CreateBuilder(tInput).SetChequeNumber(options.getChequeNo()).Build();
			if (null != options.getBankCode()) tInput = TxnInput.CreateBuilder(tInput).SetBankCode(options.getBankCode()).Build();
			if (null != options.getChequeDate()) tInput = TxnInput.CreateBuilder(tInput).SetChequeDate(options.getChequeDate().ToString()).Build();
		}
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.TXN)
				.SetMsgData(tInput.ToByteString()).Build();

		this.send(apiInput);
				
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventType() != ApiOutput.Types.EventType.TXN_RESULT.ToString()) continue;
			break;
		}
		return result;
	}

	public APIResult sendReceipt(string txnId, string mobileNo) { 
		Console.Error.WriteLine("...sendReceipt <"+txnId+">");
		
		ForwardReceiptInput receiptInput = ForwardReceiptInput.CreateBuilder()
				.SetTxnId(txnId)
				.SetCustomerMobile(mobileNo).Build();
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.FORWARD_RECEIPT)
				.SetMsgData(receiptInput.ToByteString()).Build();

		this.send(apiInput);
		APIResult result = null;
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventType() != ApiOutput.Types.EventType.FORWARD_RECEIPT_RESULT.ToString()) continue;
			break;
		}
		return result;
	}
	
	private APIResult getResult(ApiOutput apiOutput) {

		APIResult result = new APIResult();
		
		if (null == apiOutput) throw new APIException("Invalid response from EPIC. ApiOutput is null");

		result.setEventType(apiOutput.EventType.ToString());

		if ((apiOutput.OutData != null) && (!apiOutput.OutData.IsEmpty)) {

			result.setStatus(apiOutput.Status.ToString());
			result.setMessage(apiOutput.MsgText.ToString());
			try {
				StatusInfo statusInfo = StatusInfo.ParseFrom(apiOutput.OutData);

				result.setCode(statusInfo.Code);
                if (null != statusInfo.Message) result.setMessage(statusInfo.Message);
			} catch (InvalidProtocolBufferException e) {
                Console.WriteLine(e.Message);
            }

			if ((apiOutput.Status == ApiOutput.Types.ResultStatus.SUCCESS) && (apiOutput.EventType.Equals(ApiOutput.Types.EventType.TXN_RESULT))) {
				
				PaymentResult paymentResult = new PaymentResult();
				Txn txnOutput;
				try {
					txnOutput = Txn.ParseFrom(apiOutput.OutData);

					paymentResult.setPmtType(txnOutput.TxnType.ToString());
					paymentResult.setStatus(txnOutput.Status);
					paymentResult.setTxnId(txnOutput.TransactionId);
					paymentResult.setAmount(txnOutput.Amount);
					paymentResult.setSettlementStatus(txnOutput.SettlementStatus);
					paymentResult.setVoidable(txnOutput.Voidable);
					paymentResult.setChequeNo(txnOutput.ChequeNumber);
					paymentResult.setChequeDate(txnOutput.ChequeDate);
					paymentResult.setAuthCode(txnOutput.AuthCode);
					paymentResult.setCardType(txnOutput.CardBrand);
					paymentResult.setOrderId(txnOutput.OrderId);
					paymentResult.setTid(txnOutput.Tid);
					paymentResult.setMerchantId(txnOutput.Mid);

				} catch (InvalidProtocolBufferException e) {
					throw new APIException("Error reading payment result. ex="+e.Message);
				}
				result.setPaymentResult(paymentResult);
			}
		}
		
		//Console.WriteLine(result.ToString());
		return result;
	}
	
	private void send(ApiInput apiInput) {

        //Console.Write(apiInput.ToJson());
		byte[] length = new byte[4];

		try {
			length = this.intToBytes(apiInput.SerializedSize);
			//p.StandardInput.Write(length);
			//p.StandardInput.Write(apiInput.ToByteString());
			//p.StandardInput.Flush();
            output.Write(length);
            byte[] arr = apiInput.ToByteArray();
            output.Write(arr);
            //Console.Write(apiInput.ToByteArray());
            output.Flush();
		} catch (InvalidProtocolBufferException e) {
			Console.WriteLine("Parse Error " + e.ToString());
		} catch (IOException e) {
			Console.WriteLine("Error readline " + e.ToString());
		}
	}

	private ApiOutput receive() {
		
		ApiOutput apiOutput = null;
		byte[] length = new byte[4];

		try {
			this.readWithTimeout(length, 30000);
			int lengthInt = getIntegerFromByte(length);

			if (lengthInt > 0) {
				byte[] data = new byte[lengthInt];
				this.readWithTimeout(data, 30000);
				apiOutput = ApiOutput.ParseFrom(data);
			}
		} catch (InvalidProtocolBufferException e) {
			Console.WriteLine("Parse Error " + e.ToString());
		} catch (IOException e) {
			Console.WriteLine("Error readline " + e.ToString());
		}
		return apiOutput;
	}

	public int readWithTimeout(byte[] data, int timeoutMillis) {
		
        int offset = 0;
		int dataLength = data.Length;
		long maxTimeMillis = CurrentTimeMillis() + timeoutMillis;
		while (CurrentTimeMillis() < maxTimeMillis && offset < dataLength) {
			//long length = Math.Min(input.BaseStream.Length, dataLength-offset);
	    		
			// can alternatively use bufferedReader, guarded by isReady():
			int result = input.Read(data, offset, dataLength);
			if (result == -1) break;
			offset += result;
		}
		return offset;

        //input.BaseStream.ReadTimeout = timeoutMillis;
        //data = this.input.ReadBytes(data.Length);
	}

    private static readonly DateTime Jan1st1970 = new DateTime
    (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static long CurrentTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

	private string getEzecliFile() {
        return "c:\\program files\\ezetap\\ezecli\\ezecli.exe";
	}
	
	public byte[] intToBytes(int intValue) {
        byte[] intBytes = BitConverter.GetBytes(intValue);
        //if (BitConverter.IsLittleEndian)
        //    Array.Reverse(intBytes);
        byte[] result = intBytes;
	    return result;
    }

	public static byte[] reverseArray(byte[] array) {
		byte[] reversedArray = new byte[array.Length];
        for(int i = 0; i < array.Length; i++){
            reversedArray[i] = array[array.Length - i - 1];
        }
        return reversedArray;
    }
	
	public static int getIntegerFromByte(byte[] byteArr) {
		return (byteArr[3]) << 24 | (byteArr[2] & 0xFF) << 16
				| (byteArr[1] & 0xFF) << 8 | (byteArr[0] & 0xFF);
	}
}
}