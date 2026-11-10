package io.clavi.spring;

import io.clavi.admin.ClaviAdminClient;
import java.util.Collection;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;
import org.springframework.boot.autoconfigure.AutoConfiguration;
import org.springframework.boot.autoconfigure.condition.ConditionalOnClass;
import org.springframework.boot.autoconfigure.condition.ConditionalOnMissingBean;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.Customizer;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.core.authority.mapping.GrantedAuthoritiesMapper;
import org.springframework.security.oauth2.client.registration.ClientRegistration;
import org.springframework.security.oauth2.client.registration.ClientRegistrationRepository;
import org.springframework.security.oauth2.client.registration.ClientRegistrations;
import org.springframework.security.oauth2.client.registration.InMemoryClientRegistrationRepository;
import org.springframework.security.oauth2.core.ClientAuthenticationMethod;
import org.springframework.security.oauth2.core.oidc.user.OidcUserAuthority;
import org.springframework.security.oauth2.core.user.OAuth2UserAuthority;
import org.springframework.security.web.SecurityFilterChain;

@AutoConfiguration
@EnableConfigurationProperties(ClaviProperties.class)
@ConditionalOnProperty(prefix = "clavi", name = "authority")
public class ClaviAutoConfiguration {

    @Bean
    @ConditionalOnMissingBean
    public ClaviAdminClient claviAdminClient(ClaviProperties props) {
        return ClaviAdminClient.builder()
                .baseUrl(props.resolveAdminBaseUrl())
                .tenantHost(props.getTenantHost())
                .acceptAnyServerCertificate(props.isAcceptAnyServerCertificate())
                .build();
    }

    @Bean
    @ConditionalOnMissingBean
    @ConditionalOnClass(ClientRegistrationRepository.class)
    public ClientRegistrationRepository claviClientRegistrationRepository(ClaviProperties props) {
        props.validateOidc();
        ClientRegistration.Builder builder = ClientRegistrations
                .fromIssuerLocation(props.getAuthority())
                .registrationId("clavi")
                .clientId(props.getClientId())
                .redirectUri("{baseUrl}" + props.getCallbackPath())
                .scope(props.getScopes().toArray(String[]::new))
                .clientName("Clavi");

        if (props.getClientSecret() != null && !props.getClientSecret().isBlank()) {
            builder.clientSecret(props.getClientSecret())
                    .clientAuthenticationMethod(ClientAuthenticationMethod.CLIENT_SECRET_BASIC);
        } else {
            builder.clientAuthenticationMethod(ClientAuthenticationMethod.NONE);
        }

        return new InMemoryClientRegistrationRepository(builder.build());
    }

    @Bean
    @ConditionalOnMissingBean
    public GrantedAuthoritiesMapper claviAuthoritiesMapper(ClaviProperties props) {
        return authorities -> {
            if (!props.isMapRoles()) {
                return authorities;
            }
            Set<GrantedAuthority> mapped = new HashSet<>(authorities);
            for (GrantedAuthority authority : authorities) {
                if (authority instanceof OidcUserAuthority oidc) {
                    mapped.addAll(rolesFromClaim(oidc.getIdToken().getClaim("roles")));
                    if (oidc.getUserInfo() != null) {
                        mapped.addAll(rolesFromClaim(oidc.getUserInfo().getClaim("roles")));
                    }
                } else if (authority instanceof OAuth2UserAuthority oauth) {
                    mapped.addAll(rolesFromClaim(oauth.getAttributes().get("roles")));
                }
            }
            return mapped;
        };
    }

    @Configuration
    @ConditionalOnClass(SecurityFilterChain.class)
    static class ClaviSecurityConfiguration {
        @Bean
        @ConditionalOnMissingBean(SecurityFilterChain.class)
        SecurityFilterChain claviSecurityFilterChain(HttpSecurity http) throws Exception {
            http.authorizeHttpRequests(auth -> auth.anyRequest().authenticated())
                    .oauth2Login(Customizer.withDefaults())
                    .logout(Customizer.withDefaults());
            return http.build();
        }
    }

    @SuppressWarnings("unchecked")
    private static Collection<GrantedAuthority> rolesFromClaim(Object claim) {
        if (!(claim instanceof Collection<?> col)) {
            return List.of();
        }
        return col.stream()
                .filter(String.class::isInstance)
                .map(String.class::cast)
                .filter(s -> !s.isBlank())
                .map(SimpleGrantedAuthority::new)
                .collect(Collectors.toSet());
    }
}
