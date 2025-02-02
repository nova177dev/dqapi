create table dbo.poi (
    id bigint identity(1,1) not null
        constraint pk_unit primary key,
    uuid uniqueidentifier
        constraint df_unit_uuid default (newid()) not null,
    created_at datetime2
        constraint df_unit_created_at default getutcdate() null,
    --
    code varchar(128) null,
    [name] nvarchar(4000) null,
    [address] nvarchar(4000) null,
    [description] nvarchar(4000) null,
    latitude decimal(12, 9) null,
    longitude decimal(12, 9) null
)
