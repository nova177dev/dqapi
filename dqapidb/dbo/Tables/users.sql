create table dbo.users (
    id int identity(1000, 1) not null
        constraint pk_users primary key,
    uuid uniqueidentifier
        index ix_users_uuid
        constraint df_users_uuid default (newid()),
    created_at datetime
        constraint df_users_created_at default getutcdate(),
    modified_at datetime null,
    created_by varchar(128) null,
    modified_by varchar(128) null,
    --
    role_id tinyint not null
        index ix_users_role_id
        foreign key references dbo.user_roles (id),
    email varchar(255) not null
        index ix_users_email
        constraint uq_users_email unique (email),
    --is_email_confirmed bit not null
    --    constraint df_users_is_email_confirmed default (0),
    --
    full_name nvarchar(256) not null,
    title nvarchar(128) not null,
    --
    password_hash varchar(256) not null,
    password_salt varchar(256) null,
    status_id tinyint
        index ix_users_status_id
        constraint df_adp_users_status_id foreign key references dbo.user_statuses (id),
    --
    last_login datetime2 null,
    login_attempts int null
        constraint df_users_login_attempts default (0),
    search_string as '[void], ' + isnull(title, '') + ' > ' + isnull(full_name, '') + ' > ' + isnull(email, '') persisted not null
)
