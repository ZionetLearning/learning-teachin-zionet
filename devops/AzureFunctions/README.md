# Azure Functions


### How to run locally

```bash
docker-compose up --build -d
```


```bash
cd Manager
func start
```

In another terminal:

```bash
cd Accessor
func start
```

To test using curl:
```bash
curl -X POST -H "Content-Type: text/plain" -d "Hello from cURL" http://localhost:7071/api/send
```


