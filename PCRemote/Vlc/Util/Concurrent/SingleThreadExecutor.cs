/*
 * This file is part of VLCSHARP.
 *
 * VLCSHARP is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * VLCSHARP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with VLCSHARP.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * Copyright 2012 Caprica Software Limited.
 */
using System;
using System.Collections.Generic;
using System.Threading;

namespace Caprica.VlcSharp.Util.Concurrent {
 
    /**
     * A minimal implementation of a simple thread that executes a queue of tasks
     * sequentially.
     * <p>
     * This implementation has the following simplifications:
     * <ul>
     *   <li>There are no return values from runnable tasks;</li>
     *   <li>A shutdown should simply abort the worker thread - the state of the
     *       individual tasks is irrelevant with regards to shutdown;</li>
     *   <li>Any thread may add runnable tasks to the queue, but only a single
     *       instance of this class may consume tasks from its queue.</li>
     * </ul>
     * This is Old Skool synchronisation, <code>java.util.concurrent</code> is much better.
     * <p>
     * Implementation notes:
     * <ul>
     *   <li><code>lock</code> is analogous to <code>synchronized<code>;</li>
     *   <li><code>Monitor.Wait()</code> is analogous to <code>Object.wait()</code>;</li>
     *   <li><code>Monitor.Pulse()</code> is analogous to <code>Object.notifyAll()</code>.</li>
     * </ul>
     */
    public class SingleThreadExecutor {
 
        /**
         * Queue of runnable tasks.
         */
        private readonly Queue<Runnable> queue = new Queue<Runnable>();

        /**
         * Synchronisation object that must be held to query or manipulate the queue.
         */
        private readonly object queueLock = new object();

        /**
         * Background thread.
         */
        private readonly Thread thread;

        /**
         * Has a shutdown been requested?
         */
        private volatile bool shutdownRequested;
     
        /**
         * Has shutdown completed?
         */
        private volatile bool shutdownCompleted;

        /**
         * Create a new single thread executor.
         */
        public SingleThreadExecutor() {
            thread = new Thread(new ThreadStart(ThreadMainLoop));
            Logger.Debug("Starting background thread...");
            thread.Start();
            Logger.Debug("...background thread started.");
        }

        /**
         * Submit a runnable task to the executor.
         */
        public void Submit(Runnable task) {
            Logger.Trace("Submit(task={})", task);
            // Synchronise on the queue lock...
            lock(queueLock) {
                // Sanity check before adding the task
                if(!shutdownRequested) {
                    // Enqueue the new runnable task
                    queue.Enqueue(task);
                    // Fire a notification to the synchronisation object to notify any waiters
                    Logger.Trace("Notify queue of new task...");
                    Monitor.Pulse(queueLock);
                    Logger.Trace("...notified queue of new task.");
                }
                else {
                    throw new InvalidOperationException("Can not submit a task when the queue has been shut down");
                }
            }
        }

        /**
         * Shutdown the executor.
         */
        public void Shutdown() {
            Logger.Debug("Shutdown()");
            // Synchronise on the queue lock...
            lock(queueLock) {
                // Set the shutdown flag, the main loop will pick this up shortly...
                shutdownRequested = true;
                // Fire a notification to the synchronisation object to notify any waiters
                Monitor.Pulse(queueLock);
            }
        }

        /**
         * Has this executor completed its shutdown?
         *
         * @return <code>true</code> if shutdown; <code>false</code> if still running
         */
        public bool isShutdownCompleted() {
            return shutdownCompleted;
        }

        /**
         * Main thread execution loop.
         * <p>
         * The main loop will run, waiting for tasks to be added to the queue, executing
         * those tasks and looping until shutdown has been requested.
         */
        private void ThreadMainLoop() {
            Logger.Debug("Main loop starts");
            // Run the main loop until a shutdown is requested
            while(!shutdownRequested) {
                // Synchronise on the queue lock...
                lock(queueLock) {
                    // Loop waiting for items to be added to the queue, looping is necessary
                    // due to potential spurious wake-ups
                    while(queue.Count == 0 && !shutdownRequested) {
                        // Wait for a notification of a new item (or items) added to the queue
                        Logger.Trace("Waiting for queue notification...");
                        Monitor.Wait(queueLock);
                        Logger.Trace("...got queue notification.");
                    }
                }
                // Process all current items in the queue sequentially...
                while(queue.Count > 0) {
                    Runnable task = null;
                    // Synchronise on the queue since we are going to manipulate it
                    lock(queueLock) {
                        // If the queue is not empty dequeue the next item
                        if(queue.Count > 0) {
                            task = queue.Dequeue();
                        }
                    }
                    Logger.Trace("task={}", task);
                    // Execute the task...
                    if(task != null) {
                        // Ensure that an exception in the task does not break the executor
                        try {
                            task.Run();
                        }
                        catch(Exception e) {
                            Logger.Error("Runnable task threw an exception", e);
                        }
                    }
                }
            }
            // Thread main loop exits
            Logger.Debug("Main loop exits");
            shutdownCompleted = true;
        }
    }
}
