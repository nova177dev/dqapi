create table dbo.invitations
(
    id bigint identity(1,1) not null primary key,
    created_by int not null
        constraint fk_invitations_created_by foreign key (created_by) references dbo.users(id),
    created_dtm datetime2 not null
        constraint df_invitations default getdate(),
    --
    code varchar(128) not null
        index ix_invitations_code,
    is_system bit not null
        index ix_invitations_is_system
        constraint df_invitations_is_system default 0,
    used_dtm datetime2 null,
    used_by int null
        constraint fk_invitations_used_by foreign key (used_by) references dbo.users(id)
)
