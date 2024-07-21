![](.README/header.png)


# EzyReflection [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/gamedev-uv/EzyReflection/blob/main/LICENSE)

**Attempts to make reflection for Unity simpler!**

# Installation

Through the [**Unity Package Manager**](https://docs.unity3d.com/Manual/upm-ui-giturl.html) using the following Git URL :
```
https://github.com/gamedev-uv/EzyReflection.git
```

# Overview

### Member Class

#### Data Members

| Data Member       | Type        | Description                                                                                           |
|-------------------|-------------|-------------------------------------------------------------------------------------------------------|
| `Name`            | `string`    | The name of the member.                                                                               |
| `Type`            | `Type`      | The type of the member.                                                                               |
| `MemberInfo`      | `MemberInfo`| The underlying `MemberInfo` object representing the member.                                            |
| `Attributes`      | `List<object>` | List of all attributes associated with the member.                                                    |
| `Children`        | `List<Member>` | List of immediate child members of the current member.                                                 |

#### Methods

| Method                       | Parameters                                          | Description                                                                                               |
|------------------------------|-----------------------------------------------------|-----------------------------------------------------------------------------------------------------------|
| `GetValue`                   | None                                                | Retrieves the value of the member.                                                                        |
| `GetValue<T>`                | None                                                | Retrieves the value of the member.                                                                        |
| `SetValue`                   | `object value`                                      | Sets the value of the member to `value`.                                                                  |
| `GetChildren<T>`             | None                                                | Retrieves immediate child members of the current member.                                                   |
| `GetAllChildren<T>`          | None                                                | Retrieves all child members recursively of the current member.                                             |
| `HasAttribute<T>`            | None                                                | Checks if the current member has an attribute of type `T`.                                                 |
| `TryGetAttribute<T>`         | `out T attribute`                                   | Tries to retrieve the attribute of type `T` associated with the member. Returns `true` if found, `false` otherwise. |
| `FindAttributes`             | None                                                | Finds and assigns all attributes of the member.                                                            |
| `IsSearchableChild`          | `Member child`                                      | Checks if the `child` member is searchable under the current member.                                        |
| `IsValidMember`              | `MemberInfo memberInfo`                             | Checks if the `memberInfo` represents a valid member.                                                      |
| `FindChildren`               | None                                                | Finds immediate child members of the current member.                                                       |
| `FindAllChildren`            | `int maxDepth`, `int currentDepth`, `List<object> visitedObjects` | Finds all child members under the current member recursively, up to `maxDepth`.                              |
| `GetMember`                  | `MemberInfo memberInfo`                             | Returns a `Member` instance for the given `memberInfo`.                                                     |
| `FindMember<T>`              | `string memberName`, `bool searchUnderChildren`     | Finds a member with name `memberName` under this member. Optionally searches under all children.          |
| `FindMembersWithAttribute<T>` | `bool searchUnderChildren`                         | Finds all members with an attribute of type `T` under this member. Optionally searches under children.     |
| `GetMemberTitle`             | None                                                | Returns a formatted title for the member.                                                                  |
| `GetString`                  | `int indent`                                        | Returns a string representation of the member with indentation `indent`.                                   |
| `ToString`                   | None                                                | Returns a string representation of the member.                                                             |

### ReflectionHelpers Class

#### Methods

| Method                       | Parameters                                          | Description                                                                                               |
|------------------------------|-----------------------------------------------------|-----------------------------------------------------------------------------------------------------------|
| `IsSimpleType`               | `Type type`                                         | Checks if the given `type` is a simple type (primitive, enum, string, etc.).                              |
| `GetValue<T>`                | `MemberInfo member`, `object obj`                   | Retrieves the value of the `member` in the `obj` object.                                                   |
| `GetValue`                   | `MemberInfo member`, `object obj`                   | Retrieves the value of the `member` in the `obj` object.                                                   |
| `SetValue`                   | `MemberInfo member`, `object parentObject`, `object value` | Sets the `value` for the `member` in the `parentObject`.                                                 |
| `ChangeType`                 | `object obj`, `Type targetType`                     | Converts `obj` to the specified `targetType`.                                                              |
| `GetBackingField`            | `PropertyInfo property`                             | Retrieves the backing field of the `property`.                                                             |
| `GetAttributes`              | `MemberInfo memberInfo`                             | Retrieves all attributes associated with the `memberInfo`.                                                 |
| `HasAttribute<T>`            | `MemberInfo memberInfo`, `object parentObject`      | Checks if the `memberInfo` has an attribute of type `T` in the `parentObject`.                             |
