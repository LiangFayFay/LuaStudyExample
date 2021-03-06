---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by DELL.
--- DateTime: 2020/12/22 16:18
---

local controller = "Scripts.Controller."
local model = "Scripts.Model."
local view = "Scripts.View."
local global = "Scripts.Global."

-- Controllers
LoginController = require(controller .. "LoginController")
SignupController = require(controller .. "SignupController")
NormalTipController = require(controller .. "NormalTipController")
DialogController = require(controller .. "DialogController")
SuccessController = require(controller .. "SuccessController")
ThemeController = require(controller .. "ThemeController")
UserDataController = require(controller .. "UserDataController")

-- Models
LoginModel = require(model .. "LoginModel")
NormalTipModel = require(model .. "NormalTipModel")
SignupModel = require(model .. "SignupModel")
UserDataModel = require(model .. "UserDataModel")
DialogModel = require(model .. "DialogModel")
ThemeModel = require(model .. "ThemeModel")

-- Views
LoginView = require(view .. "LoginView")
SignupView = require(view .. "SignupView")
NormalTipView = require(view .. "NormalTipView")
DialogView = require(view .. "DialogView")
SuccessView = require(view .. "SuccessView")
ThemeView = require(view .. "ThemeView")

-- Global
Util = require(global .. "Util")
Json = require(global .. "Json")
IOManager = require(global .. "IOManager")
CurUser = {}
Users = {}

print("all variate init finish")

ThemeController.init()
UserDataController.init()
LoginController.init()