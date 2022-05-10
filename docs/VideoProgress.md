# Video Progress

The Url is http://localhost:3252/api/v2/Progress   

This is Json Object (With these Key/Value Pairs)  
| Name    | Description       |  Type |
|--------|-------|------|
| Video       | See [SavedVideo](SavedVideo.md)    | Object                |  
| Progress | Video Progress (0-100) | Int32
| ProgressRaw | Video Progress (0.0-1.0) | Double |
| Length | Video Length in bytes (Wrong for Mux) | Int64