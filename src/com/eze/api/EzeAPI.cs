using System;
using System.IO;
using System.Diagnostics;
using com.eze.ezecli;
using Google.ProtocolBuffers;

namespace com.eze.api {
public class EzeAPI {

	private Process p;
	private BinaryWriter output;
	private BinaryReader input;
	//private StreamReader err;
	private static EzeAPI API;

    //public EventArgs args = null;
    public event EzeNotification EzeEvent;
    public delegate void EzeNotification(String notifyMessage, EventArgs args);

    public void setMessageHandler(EzeNotification handler)
    {
        EzeEvent += handler;
    }

	private EzeAPI() {
	}
	
	/** 
	 * This method destroys the ezecli exe and gracefully closes the API.
	 */
	private void destroyInstance() {
		if (null != p) {
			try {
				p.Kill();
			} catch (Exception e) {
				Console.Write(e.Message);
			}
		}
	}
	
    public static void destroy()
    {
        if (null != API)
        {
            API.quit();
            API.destroyInstance();
        }
    }

    /**
     * Method creates an instance of EzeAPI with the given API configuration settings
     */
    public static EzeAPI create(ServerType serverType)
    {
        if (null == API)
        {
            API = new EzeAPI();
            API.initialize();
        }

        API.setServerType(serverType);
        return API;
    }

	/**
	 * This method instantiates the Ezecli and setup the input 
	 * and output buffers for reading and writing through protocol buffers.
	 */
	private void initialize() 
    {
		try 
        {
			if (null != p) 
            {
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

		} 
        catch (Exception e) 
        {
			Console.Write(e.Message);

			try 
            {
				if (p != null) p.Kill();
			} 
            catch (Exception ex) 
            {
                Console.Write(ex.Message);
		    }
			throw new EzeException("Initialize failed. e="+e.Message);
		}
	}

    private void setServerType(ServerType type)
    {
        Console.WriteLine("...Setting server type to <" + type.ToString() + ">");

        ServerTypeInput serverTypeInput = ServerTypeInput.CreateBuilder()
                        .SetServertype(MapServerType(type)).Build();

        ApiInput apiInput = ApiInput.CreateBuilder()
                        .SetMsgType(ApiInput.Types.MessageType.SERVER_TYPE)
                        .SetMsgData(serverTypeInput.ToByteString()).Build();

        this.send(apiInput);

        /*
        EzeResult result = null;

        while (true)
        {
            result = this.getResult(this.receive());
            if (result.getEventName() == EventName.SET_SERVER_TYPE) break;
        }*/
    }

	public EzeResult login(LoginMode mode, string userName, string passkey) 
    {
		Console.WriteLine("...Login User <"+mode+":"+userName+":"+passkey+">");
		
		LoginInput loginInput = LoginInput.CreateBuilder()
						.SetLoginMode(MapLoginMode(mode))
						.SetUsername(userName)
						.SetPasskey(passkey).Build();

		ApiInput apiInput = ApiInput.CreateBuilder()
						.SetMsgType(ApiInput.Types.MessageType.LOGIN)
						.SetMsgData(loginInput.ToByteString()).Build();
		
        this.send(apiInput);

        EzeResult result = null;
	
		while (true) 
        {
			result = this.getResult(this.receive());
            if (result.getEventName() != EventName.LOGIN) continue;
			if ((result.getStatus().ToString() == com.eze.ezecli.ApiOutput.Types.ResultStatus.FAILURE.ToString())) 
            {
                throw new EzeException("Login failed. " + result.ToString());
			}
			break;
		}

		return result;
	}
	
	private void quit() {
		this.logout().exit();
	}
	
	private EzeAPI logout() {
		Console.WriteLine("...logging out");
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.LOGOUT)
				.Build();

		this.send(apiInput);
		EzeResult result = null;
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventName() != EventName.LOGOUT) continue;
			if ((result.getStatus().ToString() == ApiOutput.Types.ResultStatus.FAILURE.ToString())) {
                throw new EzeException("Logout failed. " + result.ToString());
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
		EzeResult result = null;
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventName() != EventName.EXIT) continue;
			if ((result.getStatus().ToString() == ApiOutput.Types.ResultStatus.FAILURE.ToString())) {
                throw new EzeException("Exit failed. " + result.ToString());
			}
			break;
		}
		return this;
	}
	
	public EzeResult prepareDevice() {
		Console.WriteLine("...Preparing Device");
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.PREPARE_DEVICE)
				.Build();

		this.send(apiInput);
		EzeResult result = null;
		
		while (true) 
        {
			result = this.getResult(this.receive());
            if (result.getEventName() == EventName.PREPARE_DEVICE) break;
		}
		return result;
	}
	
	public EzeResult takePayment(PaymentType type, double amount, PaymentOptions options) 
    {
		EzeResult result = null;
		Console.WriteLine("...Take Payment <"+type.ToString()+",amount="+amount+","+">");
		TxnInput.Types.TxnType txnType = TxnInput.Types.TxnType.CARD_PRE_AUTH;
		
        switch(type) 
        {
    		case PaymentType.CARD: 
            {
                txnType = TxnInput.Types.TxnType.CARD_AUTH;
			    break;
            }
		    case PaymentType.CASH: 
            {
			    txnType = TxnInput.Types.TxnType.CASH;
			    break;
            }
		    case PaymentType.CHEQUE: 
            {
			    txnType = TxnInput.Types.TxnType.CHEQUE;
			    break;
            }
		    default: 
            {
			    txnType = TxnInput.Types.TxnType.CARD_PRE_AUTH;
                break;
            }
		}

        if (amount <= 0) throw new EzeException("Amount is 0 or negative");
		if (txnType == TxnInput.Types.TxnType.CHEQUE) 
        {
			if ((null == options) ||
				(null == options.getChequeNo()) || (options.getChequeNo().Length == 0) ||
				(null == options.getBankCode()) || (options.getBankCode().Length == 0) ||
				(null == options.getChequeDate())) 
            {
                    throw new EzeException("Cheque details not passed for a Cheque transaction");
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
				
		while (true) 
        {
			result = this.getResult(this.receive());

            if (result.getEventName() == EventName.TAKE_PAYMENT)
            {
                if (result.getStatus() == Status.SUCCESS) EzeEvent("Payment Successful", new EventArgs());
                else EzeEvent("Payment Failed", new EventArgs());
                break;
            }
		}
		return result;
	}

    public EzeResult sendReceipt(string txnId, string mobileNo)
    { 
		Console.Error.WriteLine("...sendReceipt <"+txnId+">");
		
		ForwardReceiptInput receiptInput = ForwardReceiptInput.CreateBuilder()
				.SetTxnId(txnId)
				.SetCustomerMobile(mobileNo).Build();
		
		ApiInput apiInput = ApiInput.CreateBuilder()
				.SetMsgType(ApiInput.Types.MessageType.FORWARD_RECEIPT)
				.SetMsgData(receiptInput.ToByteString()).Build();

		this.send(apiInput);
        EzeResult result = null;
		while (true) {
			result = this.getResult(this.receive());
			if (result.getEventName() != EventName.SEND_RECEIPT) continue;
			break;
		}
		return result;
	}

    private EzeResult getResult(ApiOutput apiOutput)
    {
        EzeResult result = new EzeResult();

        if (null == apiOutput) throw new EzeException("Invalid response from EPIC. ApiOutput is null");

        result.setEventName(MapEventName(apiOutput.EventType));
        if (apiOutput.HasStatus) result.setStatus(MapStatus(apiOutput.Status));
        if (apiOutput.HasMsgText) result.setMessage(apiOutput.MsgText);
		
        if (apiOutput.HasOutData) 
        {
    		try 
            {
				StatusInfo statusInfo = StatusInfo.ParseFrom(apiOutput.OutData);

                if (null != statusInfo)
                {
                    if (statusInfo.HasCode) result.setCode(statusInfo.Code);
                    if (statusInfo.HasMessage) result.setMessage(statusInfo.Message);
                }
			} 
            catch (InvalidProtocolBufferException e) 
            {
                Console.WriteLine(e.Message);
            }

			if ((apiOutput.Status == ApiOutput.Types.ResultStatus.SUCCESS) && (apiOutput.EventType.Equals(ApiOutput.Types.EventType.TXN_RESULT))) 
            {
				PaymentResult paymentResult = new PaymentResult();
				Txn txnOutput;
				try 
                {
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

				} 
                catch (InvalidProtocolBufferException e) 
                {
                    throw new EzeException("Error reading payment result. ex=" + e.Message);
				}
				result.setPaymentResult(paymentResult);
			}
		}

        //Console.Write("ApiOutput: " + apiOutput.ToString());

        if ((result.getEventName() == EventName.NOTIFICATION) && (null != EzeEvent))
        {
            EzeEvent(result.getMessage(), new EventArgs());
        }

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
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(intBytes);
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

    public static Status MapStatus(ApiOutput.Types.ResultStatus status)
    {
        if (status == ApiOutput.Types.ResultStatus.SUCCESS) return Status.SUCCESS;
        else return Status.FAILURE;
    }
    public static ServerTypeInput.Types.ServerType MapServerType(ServerType type)
    {
        if (type == ServerType.DEMO) return ServerTypeInput.Types.ServerType.SERVER_TYPE_DEMO;
        else return ServerTypeInput.Types.ServerType.SERVER_TYPE_PROD;
    }

    public static LoginInput.Types.LoginMode MapLoginMode(LoginMode mode)
    {
        if (mode == LoginMode.APPKEY) return LoginInput.Types.LoginMode.APPKEY;
        else return LoginInput.Types.LoginMode.PASSWORD;
    }

    public static EventName MapEventName(com.eze.ezecli.ApiOutput.Types.EventType eType)
    {
        switch (eType)
        {
            case ApiOutput.Types.EventType.LOGIN_RESULT: return EventName.LOGIN;
            case ApiOutput.Types.EventType.LOGOUT_RESULT: return EventName.LOGOUT;
            case ApiOutput.Types.EventType.EXIT_RESULT: return EventName.EXIT;
            case ApiOutput.Types.EventType.SETSERVER_RESULT: return EventName.SET_SERVER_TYPE;
            case ApiOutput.Types.EventType.PREPARE_DEVICE_RESULT: return EventName.PREPARE_DEVICE;
            case ApiOutput.Types.EventType.TXN_RESULT: return EventName.TAKE_PAYMENT;
            case ApiOutput.Types.EventType.FORWARD_RECEIPT_RESULT: return EventName.SEND_RECEIPT;
            case ApiOutput.Types.EventType.API_NOTIFICATION: return EventName.NOTIFICATION;
            case ApiOutput.Types.EventType.API_PROGRESS: return EventName.NOTIFICATION;
            default: return EventName.OTHER;
       }
    }
}
}