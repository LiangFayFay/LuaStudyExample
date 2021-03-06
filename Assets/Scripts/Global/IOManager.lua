---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by DELL.
--- DateTime: 2020/12/24 16:34
---

local IOManager = {}
local path = CS.UnityEngine.Application.dataPath .. '/Resources/UserData/'

function IOManager.load(filename)
    local file = assert(io.open(path .. filename .. '.txt', 'r'))
    local result = nil
    if file then
        local content = file:read('*all')
        if #content ~= 0 then
            result = Json:decode(content)
        end
    end

    file:close()
    return result
end

function IOManager.save(filename, content)
    local file = assert(io.open(path .. filename .. '.txt', 'w'))
    local con = Json:encode(content)
    file:write(con)
    file:close()
end

return IOManager