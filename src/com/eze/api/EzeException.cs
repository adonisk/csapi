using System;

namespace com.eze.api 
{
    public class EzeException : Exception 
    {
	    public EzeException(string msg) : base(msg) 
        {
	    }
    }
}