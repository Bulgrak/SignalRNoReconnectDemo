# SignalRNoReconnectDemo
This repository provides a minimal example showcasing an issue where the SignalR client fails to reconnect to its corresponding SignalR server after disconnection. The issue seems to be caused by the usage of `SemaphoreSlim`.

## Steps to reproduce:
1. In `SignalRNoReconnectDemoClient\Program.cs` line 59, set `ServerAddress` to the actual address of your `SignalRNoReconnectDemoServer` and then run `SignalRNoReconnectDemoClient` on computer#1.
2. Debug `SignalRNoReconnectDemoServer` from an IDE such as Visual Studio on computer#2.
3. Navigate to `SignalRNoReconnectDemoServer\Worker.cs` and:
* Place a breakpoint on line 100.
* This line contains the following code: `var result = await _serverController.SendMessageToClientAsync(new TransportMessage($"server_{testMessage.Id}", data, clientId)).ConfigureAwait(false);`
4. On `SignalRNoReconnectDemoClient`, press `Enter` to send a message to `SignalRNoReconnectDemoServer`
5. The breakpoint set in step 3 should be triggered. Do not continue.
6. First, disconnect `SignalRNoReconnectDemoClient` on computer#1 (e.g., by disabling the network adapter).
7. Remove the previously set breakpoint and resume the execution.
8. Reconnect `SignalRNoReconnectDemoClient` on computer#1.
9. Observe the behavior: The `SignalRNoReconnectDemoClient` should fail to reconnect.

## Workaround:
To temporarily resolve this issue:
* Comment out the usage of `SemaphoreSlim` in the method `_serverController_ClientListChanged` located in `SignalRNoReconnectDemoServer\Worker.cs`.
* Follow the "Steps to Reproduce" again, and at step 9, you should observe that the client can now reconnect.

## Videos demonstrating the issue
The following videos demonstrates the issue.
Note that: 
* The client is running inside a VirtualBox with Windows 10 version 22H2 OS Build 19045.3570
* The server is running on Windows 11 version 22H2 OS Build 22621.2428
### With SemaphoreSlim
https://github.com/Bulgrak/SignalRNoReconnectDemo/assets/4496566/9f07336f-e72f-4c3b-be9a-afac2c44b7df
### Without SemaphoreSlim (workaround)
https://github.com/Bulgrak/SignalRNoReconnectDemo/assets/4496566/8af5c6b6-7211-4394-99da-ad7cfc1fa0cd
