using System;

namespace Agitprop.AppHost;

public class SurrealDBResource : ContainerResource, IResourceWithConnectionString, IResourceWithEnvironment
{
    internal const string PrimaryEndpointName = "http";
    private const string DefaultUserName = "guest";

    /// <summary>
    /// Initializes a new instance of the <see cref="SurrealDBResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="userName">A parameter that contains the SurrealDB server user name, or <see langword="null"/> to use a default value.</param>
    /// <param name="password">A parameter that contains the SurrealDB server password.</param>
    public SurrealDBResource(string name, ParameterResource? userName, ParameterResource password) : base(name)
    {
        ArgumentNullException.ThrowIfNull(password);

        PrimaryEndpoint = new(this, PrimaryEndpointName);
        UserNameParameter = userName;
        PasswordParameter = password;
    }

    /// <summary>
    /// Gets the primary endpoint for the SurrealDB server.
    /// </summary>
    public EndpointReference PrimaryEndpoint { get; }

    /// <summary>
    /// Gets the parameter that contains the SurrealDB server user name.
    /// </summary>
    public ParameterResource? UserNameParameter { get; }

    internal ReferenceExpression UserNameReference =>
        UserNameParameter is not null ?
            ReferenceExpression.Create($"{UserNameParameter}") :
            ReferenceExpression.Create($"{DefaultUserName}");

    /// <summary>
    /// Gets the parameter that contains the SurrealDB server password.
    /// </summary>
    public ParameterResource PasswordParameter { get; }

    /// <summary>
    /// Gets the connection string expression for the SurrealDB server.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"Server=http://{PrimaryEndpoint.Property(EndpointProperty.HostAndPort)};Username={UserNameReference};Password={PasswordParameter};");
}