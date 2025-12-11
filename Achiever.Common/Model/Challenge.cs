using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Achiever.Model
{
    public class Challenge
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// статичная дата завершения.
        /// </summary>
        public DateTime? UntilDate { get; set; }
        public List<ChallengeAimItem> Aims { get; set; } = new List<ChallengeAimItem>();

        /// <summary>
        /// json with badge settings: color, hardness, etc
        /// </summary>
        public string BadgeSettings { get; set; }

        /// <summary>
        /// если true  то будут учитываться только достижения с начала принятия участия в состязании , а не все подряд
        /// </summary>
        public bool UseValuesAfterStartOnly { get; set; }

        /// <summary>
        /// длительноть в часах с момента принятия участия
        /// </summary>
        public int? Duration { get; set; }

        public int? OwnerId { get; set; }
        public User Owner { get; set; }
        public string XmlConfig { get; set; }
    }    
}
