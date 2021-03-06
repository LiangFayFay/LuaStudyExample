---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by DELL.
--- DateTime: 2020/12/22 11:07
---

local SignupController = {}

function SignupController.signupClick(usr, pwd01, pwd02)
    local tipText = nil
    local avail = true
    if usr == "" and avail then
        tipText = "用户名为空"
        avail = false
    end

    if (pwd01 == "" or pwd02 == "") and avail then
        tipText = "密码为空"
        avail = false
    end

    if pwd01 ~= pwd02 and avail then
        tipText = "密码不一致"
        avail = false
    end

    if UserDataController.getUser(usr) and avail then
        tipText = "用户已存在"
        avail = false
    end

    if avail then
        SignupController.onSuccess(usr, pwd01)
    else
        NormalTipController.showTip(tipText)
    end
end

function SignupController.onSuccess(usr, pwd)
    UserDataController.addUser(usr, pwd)
    SignupController.backClick()
end

function SignupController.backClick()
    SignupController.hidePanel()
    LoginController.showPanel()
end

function SignupController.hidePanel()
    SignupView.hide()
end

function SignupController.showPanel()
    SignupView.show()
end

return SignupController