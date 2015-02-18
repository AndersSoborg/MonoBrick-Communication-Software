using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
	
namespace QueueHelper
{
	public class QueueThread<T>
	{
	    private Thread thread;
		private const int maxQueueSize = 1000;
	    private readonly Queue<T> queue = new Queue<T>();
	    private readonly object queueLock = new Object();
	    private readonly Semaphore queueCounter = new Semaphore(0, maxQueueSize);
	    private readonly Action<T> handler;
	    private readonly ThreadPriority threadPriority;
	    private bool threadStopped;
	    private readonly ManualResetEvent queueDone =  new ManualResetEvent(false);
	
	    private void ThreadMain()
	    {
	      while (!threadStopped)
	      {
	        T item = default(T);
	        bool hasHandler = false;
	
	        Debug.Print("Waiting for item.");
	        queueCounter.WaitOne();
	        lock (queueLock)
	        {
	          if(queue.Count > 0)
	          {
	            item = queue.Dequeue();
	            hasHandler = true;
	          }
	        }
	
	        Thread.MemoryBarrier();
	        if(hasHandler && !threadStopped)
	        {
	          Debug.Print("Executing item.");
	          handler(item);
	        }
	        
	        Thread.MemoryBarrier();
	      }
	
	      Debug.Print("Queue done, seting event.");
	      queueDone.Set();
	    }
	
	    private void CreateThread(ThreadPriority threadPriority)
	    {
	      thread = new Thread(ThreadMain);
	      thread.IsBackground = true;
	      thread.Priority = threadPriority;
	      thread.Start();
	    }
	    public QueueThread(Action<T> handler)
	      : this(handler, ThreadPriority.Normal)
	    {
	
		}
	
	    public QueueThread(Action<T> handler, ThreadPriority threadPriority)
	    {
	      if (handler == null)
	      {
	        throw new ArgumentNullException("handler");
	      }
	
	      this.handler = handler;
	      this.threadPriority = threadPriority;
	
	      CreateThread(threadPriority);
	    }
	
	    public void Enqueue(T item)
	    {
	      Debug.Print("Adding item {0} to the queue.", item);
	      lock (queueLock)
	      {
	        queue.Enqueue(item);
	      }
	      queueCounter.Release();
	      Debug.Print("Item added.");
	    }
	
	    public void Close()
	    {
	      Debug.Print("Closing queue.");
	      threadStopped = true;
	      queueCounter.Release();
	
	      Debug.Print("Waiting for queue to close.");
	      if(!queueDone.WaitOne(10*1000))
	      {
	        Debug.Print("Failed to shut down queue thread in time.");
	        throw new TimeoutException("Failed to shut down queue thread in time.");
	      }
	      Debug.Print("Queue closed.");
	    }
	
	    public void Restart()
	    {
	      Debug.Print("Restarting queue.");
	      Close();
	
	      Debug.Print("Clearing queue.");
	      lock (queueLock)
	      {
	        queue.Clear();
	      }
	
	      Debug.Print("Starting new thread.");
	      threadStopped = false;
	      CreateThread(threadPriority);
	    }
	  }
}

