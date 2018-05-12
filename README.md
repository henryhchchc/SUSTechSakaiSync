# SUSTech Sakai Sync
Sakai resource sync client for SUSTech



## Configuration file

You should place a configuration file named `SyncConfig.xml` at the same place of the startup location.

which contains

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<SyncConfig UserName="your sid" Password="your password">
  <Resource ServerRoot="/dav/49c4f300-50cb-45d6-xxxx-f3efdb2a8732"
            LocalRoot="D:\CS307 Database">
    <Excludes>
      <Item>/tools/</Item>
    </Excludes>
  </Resource>
  <Resource ServerRoot="/dav/xxxx"
            LocalRoot="D:\CLE004 EAP A2">
  </Resource>
  <Resource ServerRoot="/dav/xxx"
            LocalRoot="D:\CS102A Java">
  </Resource>
</SyncConfig>
```

## For bugs and suggestions

Please open an issue.