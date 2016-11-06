﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UUAC.Entity;
using UUAC.Entity.DTOEntities;
using UUAC.Interface.Service;
using UUAC.WebApp.Libs;
using UUAC.WebApp.ViewModels;
using Vulcan.Core.Enities;

namespace UUAC.WebApp.Controllers
{
    public class UserController : MyControllerBase
    {
        const string rootId = "000000";
        private readonly IUserManageService _service;
        public UserController(IUserManageService service)
        {
            this._service = service;
        }
        // GET: /<controller>/
        public IActionResult List()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> QueryUserList(SearchUserModel search)
        {
            if (search.orgCode == rootId)
            {
                search.orgCode = "";
            }

            PageView view = new PageView(search.page, search.rp);
            PagedList<IUserInfo> list = await this._service.QueryUserList(search.orgCode,search.qText, view);
            var ret = JsonQTable.ConvertFromPagedList(list.DataList,list.PageIndex,list.Total, search.colkey, search.colsArray);
            return Json(ret);
        }


        public async Task<IActionResult> Edit(string userId, string pcode, string pname)
        {
            IUserInfo model;
            if (string.IsNullOrEmpty(userId))
            {
                model = new DtoUserInfo();
                if (string.IsNullOrEmpty(pcode))
                {
                    throw new ArgumentNullException("pcode", "请选择上层组织");
                }
                else
                {
                    model.OrgCode = pcode;
                    model.OrgName = pname;
                }
            }
            else
            {
                model = await this._service.GetUserInfo(userId);
                if (model == null)
                {
                    throw new ArgumentOutOfRangeException("userId", "不存在对应的用户");
                }
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> CheckUserUid(string id, [FromForm]string userId)
        {
            string ret = "";
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    bool valid = await this._service.CheckUserId(id, userId);
                    if (!valid)
                    {
                        ret = "代码已存在";
                    }
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message;
            }
            return Content(ret);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser(int type, DtoUserInfo entity)
        {
            JsonMsg msg = new JsonMsg();

            try
            {
                string errMsg;
                bool valid = ValidateUser(entity, out errMsg);
                if (!valid)
                {
                    msg.status = -1;
                    msg.message = errMsg;
                    return Json(msg);
                }
                entity.LastModifyTime = DateTime.Now;
                entity.LastModifyUserId = base.UserId;
                entity.LastModifyUserName = base.UserId;
               
                int ret = await this._service.SaveUserInfo(entity, type);
                if (ret > 0)
                {
                    msg.status = 0;
                }
                else
                {
                    msg.status = -1;
                    msg.message = "操作不成功，请稍后重试";
                }
            }
            catch (Exception ex)
            {
                msg.status = -1;
                msg.message = "操作不成功：" + ex.Message;
            }
            return Json(msg);
        }

        private static bool ValidateUser(DtoUserInfo entity, out string errMsg)
        {
            errMsg = "";
            if(entity == null)
            {
                errMsg = "用户数据为空，很检查后重试";
                return false;
            }

            if (string.IsNullOrEmpty(entity.UserUid))
            {
                errMsg += "用户标识不能为空；";
            }

            if (string.IsNullOrEmpty(entity.FullName))
            {
                errMsg += "用户姓名不能为空；";
            }

            if (string.IsNullOrEmpty(entity.UserNum))
            {
                errMsg += "用户工号不能为空；";
            }


            return string.IsNullOrEmpty(errMsg);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string userId)
        {
            JsonMsg msg = new JsonMsg();

            try
            {
                int ret = await this._service.RemoveUserInfo(userId);
                if (ret > 0)
                {
                    msg.status = 0;
                }
                else
                {
                    msg.status = -1;
                    msg.message = "操作不成功，请稍后重试";
                }
            }
            catch (Exception ex)
            {
                msg.status = -1;
                msg.message = "操作不成功：" + ex.Message;
            }
            return Json(msg);
        }

    }
}