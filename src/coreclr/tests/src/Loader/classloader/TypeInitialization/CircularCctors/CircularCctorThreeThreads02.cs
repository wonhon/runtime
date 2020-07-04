// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
A --> B --> C --> D --> E --> A
3 threads: Thread T1 starts initialization at A, thread T2 starts initialization at C, and thread T3 starts initialization at E.  
A::.cctor sleeps for a few seconds so Thread T1 is blocked.
T2 invokes C::.cctor ' D::.cctor ' E::.cctor ' A::.cctor at which point T2 becomes blocked waiting for T1.
T3 is blocked because T2 is in E::.cctor.
T1 becomes unblocked, invokes B::.cctor, wants to invoke C::.cctor but, T2 is in C::.cctor so we get a 3 thread deadlock.
We detect the deadlock and allow T1 to see C.i uninitialized state.

In B.cctor: thread T1: C.i 0
In A.cctor: thread T1: B.i 6
In E.cctor: thread T2: A.i 5
In D.cctor: thread T2: E.i 9
In C.cctor: thread T2: D.i 8
*/

using System;
using System.Threading;
using System.Runtime.CompilerServices;
public class A 
{
	public static int i;

	static A()
	{	

	       Thread.Sleep(1000*2); // 1 second
		Console.WriteLine("In A.cctor: thread {0}: B.i {1}",Thread.CurrentThread.Name,B.i);
		A.i = 5;
	}

	    // invoking this should trigger the cctor
	    [MethodImpl(MethodImplOptions.NoInlining)] 
	    public static void SomeMethod()
	    {
	        Console.WriteLine("In MyClass.SomeMethod(): thread {0}",Thread.CurrentThread.Name);    
	    }

}

public class B
{
	public static int i;
	
	static B()
	{	


		Console.WriteLine("In B.cctor: thread {0}: C.i {1}",Thread.CurrentThread.Name,C.i);
		B.i = 6;
	}

	// invoking this should trigger the cctor
	[MethodImpl(MethodImplOptions.NoInlining)] 
	public static void SomeMethod()
	{
	    Console.WriteLine("In MyClass.SomeMethod(): thread {0}",Thread.CurrentThread.Name);    
	}

}

public class C
{
	public static int i;
	
	static C()
	{	
		Console.WriteLine("In C.cctor: thread {0}: D.i {1}",Thread.CurrentThread.Name,D.i);
		C.i = 7;
	}

	// invoking this should trigger the cctor
	[MethodImpl(MethodImplOptions.NoInlining)] 
	public static void SomeMethod()
	{
	    Console.WriteLine("In MyClass.SomeMethod(): thread {0}",Thread.CurrentThread.Name);    
	}

}

public class D
{
	public static int i;

	
	static D()
	{	
		Console.WriteLine("In D.cctor: thread {0}: E.i {1}",Thread.CurrentThread.Name,E.i);
		D.i = 8;
	}

	// invoking this should trigger the cctor
	[MethodImpl(MethodImplOptions.NoInlining)] 
	public static void SomeMethod()
	{
	    Console.WriteLine("In MyClass.SomeMethod(): thread {0}",Thread.CurrentThread.Name);    
	}

}

public class E
{
	public static int i;
	
	static E()
	{	
		Console.WriteLine("In E.cctor: thread {0}: A.i {1}",Thread.CurrentThread.Name,A.i);
		E.i = 9;
	}

	// invoking this should trigger the cctor
	[MethodImpl(MethodImplOptions.NoInlining)] 
	public static void SomeMethod()
	{
	    Console.WriteLine("In MyClass.SomeMethod(): thread {0}",Thread.CurrentThread.Name);    
	}

}

public class Test
{

	public static void RunGetA()
	{
		A.SomeMethod();
	}

	public static void RunGetC()
	{
		C.SomeMethod();
	}

	public static void RunGetE()
	{
		E.SomeMethod();
	}


	public static int Main()
	{

		Thread t1 = new Thread(RunGetA);
	        t1.Name = "T1";
	        Thread t2 = new Thread(RunGetC);
	        t2.Name = "T2";
		Thread t3 = new Thread(RunGetE);
	        t3.Name = "T3";
	 
		  
			
	        t1.Start();
	        Thread.Sleep(1000*1); // 1 second
	        t2.Start();
	        Thread.Sleep(1000*1); // 1 second
	        t3.Start();

	        t3.Join();
	        t2.Join();
	        t1.Join();

		// make sure that statics were set correctly.
		if ( A.i == 5 && B.i == 6 && C.i == 7 && D.i == 8 && E.i == 9 )
		{
			Console.WriteLine("PASS");
			return 100;
		}
		else
		{
			Console.WriteLine("FAIL");
			return 101;
		}
	}
}
