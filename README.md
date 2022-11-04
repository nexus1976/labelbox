# Labelbox Coding Assessment

## Getting Started
### A couple of notes of concern while proceeding through the exercise with the presumptions taken:  
- The instructions don't indicate the precise purpose nor distinction of the properties `location` and `path` in the `assetPath` object.
- In the example given, the value _local_ given for the `location` property seems to be a sentinel value to illustrate the point that the jpeg image file 
can / will be local to the server. Subsequently, I'll not do much here with this property value other than to presume it will always be the 
value _local_ and respond with an `HTTP 400` if / when it's anything else.
- In the example given, the value _./images/foo_ given for the `path` property is pointing to a folder, but not to an actual jpeg file. I'll presume that this example intended to 
demonstrate an erroroneus request as a folder path alone doesn't specify a jpeg image file, otherwise I'll expect this property to be used to specify the full file path to a 
jpeg image file.
- The instructions don't explicitly specify this, but aside from an `HTTP 202` for when the `POST` to the `/assets/image` endpoint is successfully made, other possible 
responses will include `HTTP 500` and `HTTP 400`.
- The instructions don't indicate precisely how the webhook endpoints should be accessed. I'll presume that in each case, I will be making an `HTTP POST` to the webhook.
- On both examples where the `onFailure` payload is illustrated, it mentions that the `errors` property  
> must also return the failure reasons *in an array* in the response

The interesting thing here is that both illustrated JSON examples don't show an array of key / value pairs, rather it shows a JSON object with the specific properties named `asset`, 
`onStart`, and `onFailure` with each property being set to an array of strings with each array only containing a single element.  
This is the snippet showing the illustrated JSON:  
```json
    "errors": {
        "asset": ["is not a jpeg"],
        "onStart": ["is not a valid URL"],
        "onFailure": ["is not a valid URL"]
    }
```

To comply with the instructions as written, I will collect as many of the 5 validation rules as possible at once and provide them in an `errors` array, with each object in the 
array having a first property denoting the topic containing a validation exception (which will be from the following possible values: `asset`, `onStart`, `onSuccess`, `onFailure`) with
the second property containing the validation exception message. Consider the following example JSON snippet:
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

## Architecture

## Security Concerns

## Scalability

## How would you scale this implementation?

## Monitoring

## Thoughts on the key metrics you would track to understand the health of this service

## Future work