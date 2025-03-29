﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(Username))]
public class User : BaseEntity<Guid>
{
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;
    public int LogInFailedCount { get; set; } = 0;
    public bool Locked { get; set; } = false;
    public ISet<Role> Roles { get; set; } = new HashSet<Role>();
}