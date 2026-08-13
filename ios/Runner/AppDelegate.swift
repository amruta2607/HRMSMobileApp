import Flutter
import UIKit

@main
@objc class AppDelegate: FlutterAppDelegate, FlutterImplicitEngineDelegate {
  override func application(
    _ application: UIApplication,
    didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?
  ) -> Bool {
    return super.application(application, didFinishLaunchingWithOptions: launchOptions)
  }

  func didInitializeImplicitFlutterEngine(_ engineBridge: FlutterImplicitEngineBridge) {
    GeneratedPluginRegistrant.register(with: engineBridge.pluginRegistry)

    let messenger = engineBridge.applicationRegistrar.messenger()
    let batteryChannel = FlutterMethodChannel(
      name: "battery_optimization",
      binaryMessenger: messenger
    )

    batteryChannel.setMethodCallHandler { call, result in
      switch call.method {
      case "isLowPowerModeEnabled":
        result(ProcessInfo.processInfo.isLowPowerModeEnabled)
      case "openBatterySettings":
        // Opens iOS Settings; user turns off Low Power Mode under Battery.
        if let url = URL(string: UIApplication.openSettingsURLString) {
          UIApplication.shared.open(url, options: [:], completionHandler: nil)
          result(true)
        } else {
          result(false)
        }
      default:
        result(FlutterMethodNotImplemented)
      }
    }
  }
}
