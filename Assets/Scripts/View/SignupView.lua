---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by DELL.
--- DateTime: 2020/12/22 12:11
---

local SignupView = {}

CptCenter.signup_btn_signup.onClick:AddListener(function()
    SignupController.signupClick(CptCenter.signup_username.text,CptCenter.signup_password01.text,CptCenter.signup_password02.text)
end)

CptCenter.signup_btn_back.onClick:AddListener(function()
    SignupController.backClick()
end)

function SignupView.hide()
    CptCenter.signup_panel:SetActive(false)
    CptCenter.signup_username.text = ""
    CptCenter.signup_password01.text = ""
    CptCenter.signup_password02.text = ""
end 

function SignupView.show()
    CptCenter.signup_panel:SetActive(true)
end 

return SignupView