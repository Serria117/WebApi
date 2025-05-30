﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WebApp.Enums;

public struct Permissions
{
    public const string UserCreate = "USER.CREATE";
    public const string UserView = "USER.VIEW";
    public const string UserUpdate = "USER.UPDATE";
    public const string UserSelf = "USER.SELF";
    public const string UserDelete = "USER.DELETE";

    public const string OrgCreate = "ORG.CREATE";
    public const string OrgView = "ORG.VIEW";
    public const string OrgUpdate = "ORG.UPDATE";
    public const string OrgDelete = "ORG.DELETE";

    public const string RoleCreate = "ROLE.CREATE";
    public const string RoleView = "ROLE.VIEW";
    public const string RoleUpdate = "ROLE.UPDATE";
    public const string RoleSelf = "ROLE.SELF";
    public const string RoleDelete = "ROLE.DELETE";
    
    public const string InvoiceSync = "INVOICE.SYNC";
    public const string InvoiceQuery = "INVOICE.QUERY";
    public const string InvoiceView = "INVOICE.VIEW";
    
    public const string DocumentCreate = "DOCUMENT.CREATE";
    public const string DocumentView = "DOCUMENT.VIEW";
    public const string DocumentUpdate = "DOCUMENT.UPDATE";
    public const string DocumentDelete = "DOCUMENT.DELETE";
    public const string DocumentUpload = "DOCUMENT.ULOAD";
}
