## About

The **diff-sample** is sample scalable-ready service implemented in .NET CORE. The project demonstrates basic concepts of distributed systems where response is handled as faster as possible and heavy work is transferred to background workers.

## Design

Diff processing is build upon Difference Queue. Services push data to queue, Consumers receive data from queue in order to manage data and produce diffs. The SourceConsumer manages content received by the service. When both sides available it pushes data to the ReadyConsumer by the queue. Finally, data is being compared by pluggable difference algorithm.

## Run

In order to compile and run the service, you need to have .NET CORE 2.0 installed. Currently the only storage implemented is file storage, so it needs proper setup. By default it uses temp dir. Consider *Diffs:DataDir* section in appsettings.json.

Upload your data by POST on the endpoints:
* http://localhost:5000/v1/diff/{id}/left
* http://localhost:5000/v1/diff/{id}/right

Body:
<pre>
{
    "data": *base64-encoded data*
}
</pre>

Download diffs on:
* http://localhost:5000/v1/diff/{id}

This will give you result of comparison. Diffs are produced asyncronously that's why you need to poll the server in order to get result. When diff is not ready the result is *204 No Content*.

# Improvements

* The system lacks any logging system right now. This way we lost background worker errors. Logging is the first feature we need. Consider to use Microsoft.Extensions.Logging.
* Diff algorithm is not the best, but it is a tough work to do for the demo.
* Both sides are uploaded for comparison. It might be good decision if we work in high-load environment and want to send data immediatelly on MQ. Another solution is to send diff ID and improve algorithm to use IStorage for async upload of data.
* Add support for graceful cancellation when ASP.NET host is shutting down. Now it is possible to lose data. Solution depends on concrete environment.