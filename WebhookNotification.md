透過AddWebhookNotification自訂:
* webhook uri
* payload
* restore payload
* should notify
* custom message
* custom description

預設支援:
* Microsoft Teams
* Azure Functions
* Slack

# Webhook Uri
發送訊息的服務端點

# Payload
發送失敗訊息的內容

# Restore Payload
當服務狀態從失敗恢復時, 可以另外通知

# Should Notify
透過shouldNotify控制, 訊息是否發送

EX: 設定上班時間才發送訊息, 當服務監控連續失敗時才發送通知

# Custom Message
自訂[[FAILURE]]訊息內容

# Custom Description
自訂[[DESCRIPTIONS]]訊息內容



-----


[參考原文](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/netcore-3.1/doc/webhooks.md)