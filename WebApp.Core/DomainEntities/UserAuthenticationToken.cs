using NanoidDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WebApp.Enums;

namespace WebApp.Core.DomainEntities
{
    /// <summary>
    /// Represents an authentication token associated with a user, including its expiration and usage status.
    /// </summary>
    /// <remarks>This class is typically used to manage user authentication sessions, such as for password
    /// reset flows or multi-factor authentication. The token is linked to a specific user and includes information
    /// about its validity period and whether it has been used. Instances of this class are auditable and include
    /// creation and modification metadata inherited from the base entity.</remarks>
    [Table("UserAuthenticationToken")]
    public class UserAuthenticationToken : BaseEntityAuditable<Guid>
	{
        public new Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserId { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool Is2StepRequired { get; set; } = false;
    }
}
