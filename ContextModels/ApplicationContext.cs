using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proiect.Models;

namespace proiect.ContextModels
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoComment> PhotoComments { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupJoinRequest> GroupJoinRequests { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }

        public DbSet<AdminNotification> AdminNotifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AdminNotification>()
                .HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Group>()
                .HasMany(g => g.PendingRequests)
                .WithOne(r => r.Group)
                .HasForeignKey(r => r.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // sau NoAction

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade); // doar una cu Cascade

            builder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId);

            builder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany()
                .HasForeignKey(gm => gm.UserId);

            builder.Entity<GroupMessage>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Messages)
                .HasForeignKey(gm => gm.GroupId);

            builder.Entity<GroupMessage>()
                .HasOne(gm => gm.Sender)
                .WithMany()
                .HasForeignKey(gm => gm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Group>()
                .HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);



        }

    }
}