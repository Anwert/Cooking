if exists(select * from sys.databases where name = 'Cooking')
drop database Cooking
create database Cooking
go

use Cooking
go

if exists(select * from information_schema.tables where table_name = 'user')
	drop table [user]
create table [user]
(
	[user] int identity(1, 1) primary key,
	name varchar(max),
	password varchar(max)
)