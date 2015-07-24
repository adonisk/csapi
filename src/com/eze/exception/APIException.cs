using System;

namespace com.eze.exception {
    public class APIException : Exception 
    {
	    public APIException(string msg) : base(msg) 
        {
	    }
    }
}