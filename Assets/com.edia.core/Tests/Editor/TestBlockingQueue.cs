using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Edia.Data;

namespace Edia.Tests {

    public class TestBlockingQueue {

        [Test]
        public void EnqueueAndDequeue() {
            var bq = new BlockingQueue<int>();
            bq.Enqueue(42);
            Assert.AreEqual(1, bq.NumItems());

            int result = bq.Dequeue();
            Assert.AreEqual(42, result);
            Assert.AreEqual(0, bq.NumItems());
        }

        [Test]
        public void FIFOOrder() {
            var bq = new BlockingQueue<string>();
            bq.Enqueue("first");
            bq.Enqueue("second");
            bq.Enqueue("third");

            Assert.AreEqual("first", bq.Dequeue());
            Assert.AreEqual("second", bq.Dequeue());
            Assert.AreEqual("third", bq.Dequeue());
        }

        [Test]
        public void EnqueueNullThrows() {
            var bq = new BlockingQueue<string>();
            Assert.Throws<ArgumentNullException>(() => bq.Enqueue(null));
        }

        [Test]
        public void NumItemsTracksCorrectly() {
            var bq = new BlockingQueue<int>();
            Assert.AreEqual(0, bq.NumItems());

            bq.Enqueue(1);
            bq.Enqueue(2);
            Assert.AreEqual(2, bq.NumItems());

            bq.Dequeue();
            Assert.AreEqual(1, bq.NumItems());

            bq.Dequeue();
            Assert.AreEqual(0, bq.NumItems());
        }

        [Test]
        public void DequeueBlocksUntilEnqueue() {
            var bq = new BlockingQueue<int>();
            int result = -1;
            bool dequeued = false;

            // Start a consumer that blocks
            var consumer = Task.Run(() => {
                result = bq.Dequeue();
                dequeued = true;
            });

            // Give consumer time to start blocking
            Thread.Sleep(50);
            Assert.IsFalse(dequeued);

            // Enqueue to unblock
            bq.Enqueue(99);
            consumer.Wait(1000);

            Assert.IsTrue(dequeued);
            Assert.AreEqual(99, result);
        }

        [Test]
        public void ProducerConsumerPattern() {
            var bq = new BlockingQueue<int>();
            var results = new List<int>();
            int itemCount = 100;

            // Consumer task
            var consumer = Task.Run(() => {
                for (int i = 0; i < itemCount; i++) {
                    results.Add(bq.Dequeue());
                }
            });

            // Producer
            for (int i = 0; i < itemCount; i++) {
                bq.Enqueue(i);
            }

            consumer.Wait(5000);
            Assert.AreEqual(itemCount, results.Count);

            // Verify FIFO order
            for (int i = 0; i < itemCount; i++) {
                Assert.AreEqual(i, results[i]);
            }
        }

        [Test]
        public void MultipleProducers() {
            var bq = new BlockingQueue<int>();
            var results = new List<int>();
            object lockObj = new object();
            int totalItems = 200;

            // Consumer
            var consumer = Task.Run(() => {
                for (int i = 0; i < totalItems; i++) {
                    int val = bq.Dequeue();
                    lock (lockObj) {
                        results.Add(val);
                    }
                }
            });

            // Two producers, each producing 100 items
            var p1 = Task.Run(() => {
                for (int i = 0; i < 100; i++) bq.Enqueue(i);
            });
            var p2 = Task.Run(() => {
                for (int i = 100; i < 200; i++) bq.Enqueue(i);
            });

            Task.WaitAll(p1, p2);
            consumer.Wait(5000);

            Assert.AreEqual(totalItems, results.Count);
        }
    }
}
