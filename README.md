# Labelbox Coding Assessment

## Getting Started
### Running the Assessment
This assessment presumes that local port `5888` is free / available.
After cloning this git repo locally, you may issue any of the following commands in the root of this local repo's directory:
- `make test-e2e`  *this will run all the unit tests* 
- `make test TESTFILTER={testfilter}`  *this will run all of the tests with test names matching the given {testfilter} parameter (e.g., `make test TESTFILTER=WhenCalling_PostAsset_Returns500`)* 
- `make server`  *this will simply instantiate the microservice which will make the service available at `http://localhost:5888`*  
You can also observe an OpenAPI spec at `http://localhost:5888/swagger/index.html`

The assessment didn't require the provision for the webhooks themselves. Given this, it's worth noting that for any microservice calls to be successful, the webhooks specified in the `/assets/image` payload will need to be reachable by the microservice, which is to say that they will need to be reachable to the Docker container. If another local service is to be used, it should be in a Docker container configured to utilize the network named `labelbox-net` (using the `bridge` driver) and then accessed via the Docker service name.

For local testing, while I didn't include it in this assessment, I nevertheless created a webhooks project to receive the webhook calls which is located at this
git repo: https://github.com/nexus1976/labelbox-webhooks . If using this project for testing the webhooks, please initiate it by running a `docker compose up` in the repo's root folder. This project assumes that local port `3030` is free / available.

Since the microservice is also performing local file io, it's worth noting that in the root of this repo, there is a subfolder called `images`. This folder is mounted to the microservice container to the path `/images`. You'll note that I've seeded a few image files in there already for testing, however for any additional image files you wish to test with, please copy them into this folder on your host before attempting to reference them in the `/assets/image` payload.

The following would be an example payload to the `assets/image` endpoint if using the `labelbox-webhooks` from above (presuming a `curl --request POST 'http://localhost:5888/assets/image' --header 'Content-Type: application/json'` the following would go into the `--data-raw` string value):
```json
{
    "assetPath": {
        "location": "local",
        "path": "/images/good.jpg"
    },
    "notifications": {
     "onStart": "http://labelbox-webhooks:3030/onstart",
     "onSuccess": "http://labelbox-webhooks:3030/onsuccess",
     "onFailure": "http://labelbox-webhooks:3030/onfailure"
    }
}
```

### Some Noteworthy Design Considerations:  
- The instructions don't indicate the precise distinction of the properties `location` and `path` in the `assetPath` object.
- In the example given, the value _local_ given for the `location` property seems to be a sentinel value to illustrate the point that the jpeg image file 
can / will be local to the server. Subsequently, I'll not do much here with this property value other than to presume it will always be the 
value _local_ and respond with an `HTTP 400` if / when it's anything else.
- In the example given, the value _./images/foo_ given for the `path` property is pointing to a folder, but not to an actual jpg file. I'll presume that this example intended to demonstrate an erroneous request as a folder path alone doesn't specify a jpg image file...otherwise I'll expect this property to be used to specify the full file path to a jpg image file.
- The instructions don't explicitly specify this, but aside from an `HTTP 202` for when the `POST` to the `/assets/image` endpoint is successfully made, other possible responses will include `HTTP 500` and `HTTP 400`.
- The instructions don't indicate precisely how the webhook endpoints should be accessed. I'll presume that in each case, I will be making an `HTTP POST` to the webhooks.
- On both examples where the `onFailure` payload is illustrated, it mentions that the `errors` property  
> must also return the failure reasons *in an array* in the response

The interesting thing here is that both illustrated JSON examples don't show the `errors` property with an *array* of key / value pairs, rather it shows a *JSON object* with the specific properties named `asset`, `onStart`, and `onFailure` , and each property being set to an array of strings containing only a single element.  
This is the snippet showing the illustrated JSON:  
```json
    "errors": {
        "asset": ["is not a jpeg"],
        "onStart": ["is not a valid URL"],
        "onFailure": ["is not a valid URL"]
    }
```

To comply with the instructions as written, I will collect as many of the validation rules as possible and provide them in an `errors` *array*, with each object in the array having a first property denoting the topic containing a validation exception (which will be from the following possible values: `asset`, `onStart`, `onSuccess`, `onFailure`) with the second property containing the validation exception message. 
Consider the following example JSON snippet:
```json
    "errors": [
        { "asset": "is not a jpeg" },
        { "onStart": "is not a valid URL" },
        { "onSuccess": "is not a valid URL" },
        { "onFailure": "is not a valid URL" }
    ]
```

## Running Tests
It's worth noting that this assessment is using an in-memory database and so entries into it will not survive service restarts.
The following are the list of the available unit test names:
- WhenCalling_GetAsset_Returns200
- WhenCalling_GetAsset_Returns400
- WhenCalling_GetAsset_Returns404
- WhenCalling_GetAsset_Returns500
- WhenCalling_PostAsset_Returns202
- WhenCalling_PostAsset_Returns400
- WhenCalling_PostAsset_Returns500
- WhenCalling_ConvertEnumToString_AllValues
- WhenCalling_GetErrorsCollection_WithAllFailures
- WhenCalling_GetErrorsCollection_WithNoFailures
- WhenCalling_TrySendFailureEventAsync_WithBadURL
- WhenCalling_TrySendFailureEventAsync_WithGoodURL
- WhenCalling_TrySendStartedEventAsync_WithBadURL
- WhenCalling_TrySendStartedEventAsync_WithGoodURL
- WhenCalling_TrySendSuccessEventAsync_WithBadURL
- WhenCalling_TrySendSuccessEventAsync_WithGoodURL
- WhenCalling_ValidateJPEG_WithAnUnreachableFile
- WhenCalling_ValidateJPEG_WithGoodJPG
- WhenCalling_ValidateJPEG_WithNotAJPG
- WhenCalling_ValidateJPEG_WithTooSmallJPG
- WhenCalling_ValidateURLs_AllGoodUrls
- WhenCalling_ValidateURLs_BadOnFailureURL
- WhenCalling_ValidateURLs_BadOnStartURL
- WhenCalling_ValidateURLs_BadOnSuccessURL
- WhenCalling_Dequeue
- WhenCalling_Enqueue
- WhenCalling_ProcessMessageAsync_OneItem

You can run all unit tests by issuing a `make test-e2e` command in root of this local repo's directory.

## Architecture
While this project is leveraging C# and the dotnet 6 framework, its principles are ubiquitous and easily able to be implemented on most any modern stack / language. In this project many concepts are leveraged and demonstrated, some of which include asynchronous processing, thread safety between web processes and background processes (primarily surrounding a thread-safe queuing mechanism), the use of OR/Ms, dependency injection, jpg-specific analysis, and of course all of the typical concepts in modern unit testing (e.g., mocking, testing behavior rather than implementation, AAA syntax, etc.)

## Security Concerns
If addressing this project as a potential "productionalized" service, the following related to security would immediately arise regarding the microservice:
- The service is not presently served via https. In order to make the communications private, an SSL/TSL cert would need to be properly leveraged.
- The service does not presently have any authorization nor authentication requirements. If this service were to be leveraged by tenants, then a typical OIDC-based jwt token would typically be used to identify properly-authenticated tenants.
- The accessing of local file io would also be problematic. Either a Base64-encoded stream of the potential jpg image file would need to be sent with the `/assets/image` endpoint, or a secured file location would need to be established such that tenants could securely upload potential jpg images to then reference in the payload while multi-tenancy is respected.
- The webhooks also do not contain any ability to be secured presently. If the premise is that these webhooks are tenant-specific endpoints, then an authentication mechanism would also need to be established for these webhooks to be called in a secured manner (including the requirement of utilizing https).

## Scalability
The microservice in this project is not very scalable for a variety of reasons (although proving it to be containerized is a good first step). Firstly, given the file io approach, there can really only be one instance of this service running at any given time, otherwise callers would have to know to which service instance to upload images (and the direct locations of those instance directories) in order to engage with that specific instance's endpoints. Secondly, the service here is not using a shared / distributed data store of any sort, and so the queuing mechanism while thread-safe, isn't durable.

## How would you scale this implementation?
Going hand-in-hand with the security concerns, I would first separate the file io away from local file access. I might explore the use-cases involved to determine the best path for clients in order to decide between a shared file server where tenants could upload their files before calling the microservice (which is a two-step operation for callers) versus eliminating shared file io completely by accepting the file directly in the incoming payload itself (which is a one-step operation for callers). In this latter approach, the file coming in through the call would still need to be persisted somewhere in order for the queue processing to be able to pick it up and process, but said persistence wouldn't need to be shared with clients and could be scaled internally as needed.

Next I would also leverage a shared persistence mechanism for these incoming requests, whether that be a database, or a simple persistable-queue. The persistence of the jpg file from the last point here might help shape this decision as well. In either case, any persistence mechanism chosen would need to be horizontally scalable, persistable, and would need to be able to handle multiple processes contention such that multiple queue-handling processes could be actively watching for incoming traffic to handle. This might mean the use of an AMQP-compliant queue like RabbitMQ, or something more robust for streaming like Kafka, or a database like MongoDB or Couchbase.

That leads me to the last part in that I would most-likely bifurcate the microservice from the queue processing in order to scale those two parts independently. In this way, the microservice is a far simpler microservice and can be monitored, scaled, and provide SLAs independently of the queue processing. This also alleviates any threading concern or thread-sharing issues with the queue processing coexisting with the microservice in the same process / memory space. That's not to say that all of the concerns that go into asynchronous processing go away, it just allows the microservice and the queue processing to not have to deal with resource contention in the same manner as it presently does.

## Monitoring
This service currently has a standard `/healthz` endpoint. While it can be used to determine if the service is up and if it can healthily access it's in-memory datastore, it's still severely limited. For more robust monitoring, I would conform the service to include the more updated `/livez` and `/readyz` healthchecks equipped with all of the expected options (e.g., the `verbose` querystring). In consideration of the aforementioned section mentioning how to scale, this would be an axiom across all services to ensure monitoring capabilities are made available. This all presumes the use of one of the many monitoring solutions available (e.g. Prometheus, New Relic, SolarWinds, etc.)

In addition to real-time monitoring, the service has an entry-level of a logging abstraction that's leveraged. This would need to be more extensively used throughout the code and a robust logging platform adopting (e.g., an ELK-stack tool, Graylog, etc.)

## Thoughts on the key metrics you would track to understand the health of this service
Outside of the normal band of errors and simple service-availability, things like memory / CPU / and disk contention would be beneficial to collect metrics for such that an auto-scaling tool like KEDA could be leveraged.

## Future work
This service could be envisioned to be used for an expanding set of image formats beyond jpg as well as any additional validation requirements that were necessary (which might also help in raising the quality level of the image analysis itself as my current implementation here seems a bit lacking under certain scenarios). It might also be something by where further maturation in the RMM might be implemented such that corresponding / related endpoints could be included in the response payloads (e.g., where they can pull their final analysis for their images from, or even something as simple as the `/assets/{assetId}` endpoint being specified in the response payload of the `/assets/image` call). Another consideration might be if / when it makes sense to explore the use of gRPC in some of these processes (with perhaps a gRPC version of this service being made available).