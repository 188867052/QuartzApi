using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Quartz.Api;

namespace Entities
{
    public partial class  QuartzDbContext : DbContext
    {
        public  QuartzDbContext()
        {
        }

        public  QuartzDbContext(DbContextOptions< QuartzDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<QrtzBlobTriggers> QrtzBlobTriggers { get; set; }

        public virtual DbSet<QrtzCalendars> QrtzCalendars { get; set; }

        public virtual DbSet<QrtzCronTriggers> QrtzCronTriggers { get; set; }

        public virtual DbSet<QrtzFiredTriggers> QrtzFiredTriggers { get; set; }

        public virtual DbSet<QrtzJobDetails> QrtzJobDetails { get; set; }

        public virtual DbSet<QrtzLocks> QrtzLocks { get; set; }

        public virtual DbSet<QrtzPausedTriggerGrps> QrtzPausedTriggerGrps { get; set; }

        public virtual DbSet<QrtzSchedulerState> QrtzSchedulerState { get; set; }

        public virtual DbSet<QrtzSimpleTriggers> QrtzSimpleTriggers { get; set; }

        public virtual DbSet<QrtzSimpropTriggers> QrtzSimpropTriggers { get; set; }

        public virtual DbSet<QrtzTriggers> QrtzTriggers { get; set; }

        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(EFCore.Scaffolding.Extension.Connection.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QrtzBlobTriggers>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.TriggerName, e.TriggerGroup });

                entity.ToTable("QRTZ_BLOB_TRIGGERS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.TriggerName)
                    .HasColumnName("TRIGGER_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.TriggerGroup)
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.BlobData).HasColumnName("BLOB_DATA");
            });

            modelBuilder.Entity<QrtzCalendars>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.CalendarName });

                entity.ToTable("QRTZ_CALENDARS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.CalendarName)
                    .HasColumnName("CALENDAR_NAME")
                    .HasMaxLength(200);

                entity.Property(e => e.Calendar)
                    .IsRequired()
                    .HasColumnName("CALENDAR");
            });

            modelBuilder.Entity<QrtzCronTriggers>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.TriggerName, e.TriggerGroup });

                entity.ToTable("QRTZ_CRON_TRIGGERS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.TriggerName)
                    .HasColumnName("TRIGGER_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.TriggerGroup)
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.CronExpression)
                    .IsRequired()
                    .HasColumnName("CRON_EXPRESSION")
                    .HasMaxLength(120);

                entity.Property(e => e.TimeZoneId)
                    .HasColumnName("TIME_ZONE_ID")
                    .HasMaxLength(80);

                entity.HasOne(d => d.QrtzTriggers)
                    .WithOne(p => p.QrtzCronTriggers)
                    .HasForeignKey<QrtzCronTriggers>(d => new { d.SchedName, d.TriggerName, d.TriggerGroup })
                    .HasConstraintName("FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS");
            });

            modelBuilder.Entity<QrtzFiredTriggers>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.EntryId });

                entity.ToTable("QRTZ_FIRED_TRIGGERS");

                entity.HasIndex(e => new { e.SchedName, e.InstanceName })
                    .HasName("IDX_QRTZ_FT_TRIG_INST_NAME");

                entity.HasIndex(e => new { e.SchedName, e.JobGroup })
                    .HasName("IDX_QRTZ_FT_JG");

                entity.HasIndex(e => new { e.SchedName, e.TriggerGroup })
                    .HasName("IDX_QRTZ_FT_TG");

                entity.HasIndex(e => new { e.SchedName, e.InstanceName, e.RequestsRecovery })
                    .HasName("IDX_QRTZ_FT_INST_JOB_REQ_RCVRY");

                entity.HasIndex(e => new { e.SchedName, e.JobName, e.JobGroup })
                    .HasName("IDX_QRTZ_FT_J_G");

                entity.HasIndex(e => new { e.SchedName, e.TriggerName, e.TriggerGroup })
                    .HasName("IDX_QRTZ_FT_T_G");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.EntryId)
                    .HasColumnName("ENTRY_ID")
                    .HasMaxLength(140);

                entity.Property(e => e.FiredTime).HasColumnName("FIRED_TIME");

                entity.Property(e => e.InstanceName)
                    .IsRequired()
                    .HasColumnName("INSTANCE_NAME")
                    .HasMaxLength(200);

                entity.Property(e => e.IsNonconcurrent).HasColumnName("IS_NONCONCURRENT");

                entity.Property(e => e.JobGroup)
                    .HasColumnName("JOB_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.JobName)
                    .HasColumnName("JOB_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.Priority).HasColumnName("PRIORITY");

                entity.Property(e => e.RequestsRecovery).HasColumnName("REQUESTS_RECOVERY");

                entity.Property(e => e.SchedTime).HasColumnName("SCHED_TIME");

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasColumnName("STATE")
                    .HasMaxLength(16);

                entity.Property(e => e.TriggerGroup)
                    .IsRequired()
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.TriggerName)
                    .IsRequired()
                    .HasColumnName("TRIGGER_NAME")
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<QrtzJobDetails>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.JobName, e.JobGroup });

                entity.ToTable("QRTZ_JOB_DETAILS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.JobName)
                    .HasColumnName("JOB_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.JobGroup)
                    .HasColumnName("JOB_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(250);

                entity.Property(e => e.IsDurable).HasColumnName("IS_DURABLE");

                entity.Property(e => e.IsNonconcurrent).HasColumnName("IS_NONCONCURRENT");

                entity.Property(e => e.IsUpdateData).HasColumnName("IS_UPDATE_DATA");

                entity.Property(e => e.JobClassName)
                    .IsRequired()
                    .HasColumnName("JOB_CLASS_NAME")
                    .HasMaxLength(250);

                entity.Property(e => e.JobData).HasColumnName("JOB_DATA");

                entity.Property(e => e.RequestsRecovery).HasColumnName("REQUESTS_RECOVERY");
            });

            modelBuilder.Entity<QrtzLocks>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.LockName });

                entity.ToTable("QRTZ_LOCKS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.LockName)
                    .HasColumnName("LOCK_NAME")
                    .HasMaxLength(40);
            });

            modelBuilder.Entity<QrtzPausedTriggerGrps>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.TriggerGroup });

                entity.ToTable("QRTZ_PAUSED_TRIGGER_GRPS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.TriggerGroup)
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<QrtzSchedulerState>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.InstanceName });

                entity.ToTable("QRTZ_SCHEDULER_STATE");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.InstanceName)
                    .HasColumnName("INSTANCE_NAME")
                    .HasMaxLength(200);

                entity.Property(e => e.CheckinInterval).HasColumnName("CHECKIN_INTERVAL");

                entity.Property(e => e.LastCheckinTime).HasColumnName("LAST_CHECKIN_TIME");
            });

            modelBuilder.Entity<QrtzSimpleTriggers>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.TriggerName, e.TriggerGroup });

                entity.ToTable("QRTZ_SIMPLE_TRIGGERS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.TriggerName)
                    .HasColumnName("TRIGGER_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.TriggerGroup)
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.RepeatCount).HasColumnName("REPEAT_COUNT");

                entity.Property(e => e.RepeatInterval).HasColumnName("REPEAT_INTERVAL");

                entity.Property(e => e.TimesTriggered).HasColumnName("TIMES_TRIGGERED");

                entity.HasOne(d => d.QrtzTriggers)
                    .WithOne(p => p.QrtzSimpleTriggers)
                    .HasForeignKey<QrtzSimpleTriggers>(d => new { d.SchedName, d.TriggerName, d.TriggerGroup })
                    .HasConstraintName("FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS");
            });

            modelBuilder.Entity<QrtzSimpropTriggers>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.TriggerName, e.TriggerGroup });

                entity.ToTable("QRTZ_SIMPROP_TRIGGERS");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.TriggerName)
                    .HasColumnName("TRIGGER_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.TriggerGroup)
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.BoolProp1).HasColumnName("BOOL_PROP_1");

                entity.Property(e => e.BoolProp2).HasColumnName("BOOL_PROP_2");

                entity.Property(e => e.DecProp1)
                    .HasColumnName("DEC_PROP_1")
                    .HasColumnType("numeric(13, 4)");

                entity.Property(e => e.DecProp2)
                    .HasColumnName("DEC_PROP_2")
                    .HasColumnType("numeric(13, 4)");

                entity.Property(e => e.IntProp1).HasColumnName("INT_PROP_1");

                entity.Property(e => e.IntProp2).HasColumnName("INT_PROP_2");

                entity.Property(e => e.LongProp1).HasColumnName("LONG_PROP_1");

                entity.Property(e => e.LongProp2).HasColumnName("LONG_PROP_2");

                entity.Property(e => e.StrProp1)
                    .HasColumnName("STR_PROP_1")
                    .HasMaxLength(512);

                entity.Property(e => e.StrProp2)
                    .HasColumnName("STR_PROP_2")
                    .HasMaxLength(512);

                entity.Property(e => e.StrProp3)
                    .HasColumnName("STR_PROP_3")
                    .HasMaxLength(512);

                entity.Property(e => e.TimeZoneId)
                    .HasColumnName("TIME_ZONE_ID")
                    .HasMaxLength(80);

                entity.HasOne(d => d.QrtzTriggers)
                    .WithOne(p => p.QrtzSimpropTriggers)
                    .HasForeignKey<QrtzSimpropTriggers>(d => new { d.SchedName, d.TriggerName, d.TriggerGroup })
                    .HasConstraintName("FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS");
            });

            modelBuilder.Entity<QrtzTriggers>(entity =>
            {
                entity.HasKey(e => new { e.SchedName, e.TriggerName, e.TriggerGroup });

                entity.ToTable("QRTZ_TRIGGERS");

                entity.HasIndex(e => new { e.SchedName, e.CalendarName })
                    .HasName("IDX_QRTZ_T_C");

                entity.HasIndex(e => new { e.SchedName, e.JobGroup })
                    .HasName("IDX_QRTZ_T_JG");

                entity.HasIndex(e => new { e.SchedName, e.NextFireTime })
                    .HasName("IDX_QRTZ_T_NEXT_FIRE_TIME");

                entity.HasIndex(e => new { e.SchedName, e.TriggerGroup })
                    .HasName("IDX_QRTZ_T_G");

                entity.HasIndex(e => new { e.SchedName, e.TriggerState })
                    .HasName("IDX_QRTZ_T_STATE");

                entity.HasIndex(e => new { e.SchedName, e.JobName, e.JobGroup })
                    .HasName("IDX_QRTZ_T_J");

                entity.HasIndex(e => new { e.SchedName, e.MisfireInstr, e.NextFireTime })
                    .HasName("IDX_QRTZ_T_NFT_MISFIRE");

                entity.HasIndex(e => new { e.SchedName, e.TriggerGroup, e.TriggerState })
                    .HasName("IDX_QRTZ_T_N_G_STATE");

                entity.HasIndex(e => new { e.SchedName, e.TriggerState, e.NextFireTime })
                    .HasName("IDX_QRTZ_T_NFT_ST");

                entity.HasIndex(e => new { e.SchedName, e.MisfireInstr, e.NextFireTime, e.TriggerState })
                    .HasName("IDX_QRTZ_T_NFT_ST_MISFIRE");

                entity.HasIndex(e => new { e.SchedName, e.TriggerName, e.TriggerGroup, e.TriggerState })
                    .HasName("IDX_QRTZ_T_N_STATE");

                entity.HasIndex(e => new { e.SchedName, e.MisfireInstr, e.NextFireTime, e.TriggerGroup, e.TriggerState })
                    .HasName("IDX_QRTZ_T_NFT_ST_MISFIRE_GRP");

                entity.Property(e => e.SchedName)
                    .HasColumnName("SCHED_NAME")
                    .HasMaxLength(120);

                entity.Property(e => e.TriggerName)
                    .HasColumnName("TRIGGER_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.TriggerGroup)
                    .HasColumnName("TRIGGER_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.CalendarName)
                    .HasColumnName("CALENDAR_NAME")
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(250);

                entity.Property(e => e.EndTime)
                    .HasConversion(new DateTimeToTicksConverter())
                    .HasColumnName("END_TIME");

                entity.Property(e => e.JobData).HasColumnName("JOB_DATA");

                entity.Property(e => e.JobGroup)
                    .IsRequired()
                    .HasColumnName("JOB_GROUP")
                    .HasMaxLength(150);

                entity.Property(e => e.JobName)
                    .IsRequired()
                    .HasColumnName("JOB_NAME")
                    .HasMaxLength(150);

                entity.Property(e => e.MisfireInstr).HasColumnName("MISFIRE_INSTR");

                entity.Property(e => e.NextFireTime)
                    .HasConversion(new DateTimeToTicksConverter())
                    .HasColumnName("NEXT_FIRE_TIME");

                entity.Property(e => e.PrevFireTime)
                    .HasConversion(new DateTimeToTicksConverter())
                    .HasColumnName("PREV_FIRE_TIME");

                entity.Property(e => e.Priority).HasColumnName("PRIORITY");

                entity.Property(e => e.StartTime)
                    .HasConversion(new DateTimeToTicksConverter())
                    .HasColumnName("START_TIME");

                entity.Property(e => e.TriggerState)
                    .IsRequired()
                    .HasColumnName("TRIGGER_STATE")
                    .HasMaxLength(16);

                entity.Property(e => e.TriggerType)
                    .IsRequired()
                    .HasColumnName("TRIGGER_TYPE")
                    .HasMaxLength(8);

                entity.HasOne(d => d.QrtzJobDetails)
                    .WithMany(p => p.QrtzTriggers)
                    .HasForeignKey(d => new { d.SchedName, d.JobName, d.JobGroup })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.CreateTime).HasColumnType("datetime");

                entity.Property(e => e.DisplayName).HasMaxLength(50);

                entity.Property(e => e.LoginName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .HasConversion(new ValueConverter<string, string>(o => Helper.Encrypt(o), o => Helper.Decrypt(o)))
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
