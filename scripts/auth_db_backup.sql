--
-- PostgreSQL database dump
--

\restrict knqlced6AarPXaiH4tRZjazywHZBBxSieAzKWBgyumxtlYmrr7gcETKZAnJHs4q

-- Dumped from database version 16.14
-- Dumped by pg_dump version 16.14

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: password_reset_tokens; Type: TABLE; Schema: public; Owner: flowfi
--

CREATE TABLE public.password_reset_tokens (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    token character varying(255) NOT NULL,
    otp_code character varying(10),
    expires_at timestamp without time zone NOT NULL,
    is_used boolean DEFAULT false NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.password_reset_tokens OWNER TO flowfi;

--
-- Name: refresh_tokens; Type: TABLE; Schema: public; Owner: flowfi
--

CREATE TABLE public.refresh_tokens (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    token text NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    is_revoked boolean DEFAULT false NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.refresh_tokens OWNER TO flowfi;

--
-- Name: user_devices; Type: TABLE; Schema: public; Owner: flowfi
--

CREATE TABLE public.user_devices (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    device_id character varying(255) NOT NULL,
    device_name character varying(255),
    platform character varying(50),
    push_token text,
    last_synced_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT user_devices_platform_check CHECK (((platform)::text = ANY ((ARRAY['ANDROID'::character varying, 'IOS'::character varying])::text[])))
);


ALTER TABLE public.user_devices OWNER TO flowfi;

--
-- Name: user_logs; Type: TABLE; Schema: public; Owner: flowfi
--

CREATE TABLE public.user_logs (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    user_id uuid NOT NULL,
    action character varying(100) NOT NULL,
    status character varying(20) NOT NULL,
    ip_address character varying(45),
    user_agent text,
    failure_reason text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT user_logs_action_check CHECK (((action)::text = ANY ((ARRAY['REGISTER'::character varying, 'LOGIN'::character varying, 'LOGOUT'::character varying, 'CHANGE_PASSWORD'::character varying, 'UPDATE_PROFILE'::character varying, 'FORGOT_PASSWORD'::character varying, 'RESET_PASSWORD'::character varying])::text[]))),
    CONSTRAINT user_logs_status_check CHECK (((status)::text = ANY ((ARRAY['SUCCESS'::character varying, 'FAILED'::character varying])::text[])))
);


ALTER TABLE public.user_logs OWNER TO flowfi;

--
-- Name: users; Type: TABLE; Schema: public; Owner: flowfi
--

CREATE TABLE public.users (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    email character varying(255) NOT NULL,
    password_hash text,
    full_name character varying(255),
    avatar_url text,
    date_of_birth date,
    currency_code character varying(10) DEFAULT 'VND'::character varying NOT NULL,
    monthly_budget_limit numeric(18,2),
    auth_provider character varying(20) DEFAULT 'LOCAL'::character varying NOT NULL,
    is_verified boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp with time zone,
    deleted_at timestamp with time zone
);


ALTER TABLE public.users OWNER TO flowfi;

--
-- Data for Name: password_reset_tokens; Type: TABLE DATA; Schema: public; Owner: flowfi
--

COPY public.password_reset_tokens (id, user_id, token, otp_code, expires_at, is_used, created_at) FROM stdin;
22222222-2222-2222-2222-222222222222	aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa	reset_token_xyz	123456	2026-06-20 23:59:59	f	2026-06-19 17:42:35.327769
\.


--
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: public; Owner: flowfi
--

COPY public.refresh_tokens (id, user_id, token, expires_at, is_revoked, created_at) FROM stdin;
11111111-1111-1111-1111-111111111111	aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa	refresh_token_example_abc123...	2026-07-19 23:59:59	f	2026-06-19 17:42:35.319969
13d1bdbf-e62c-48e5-ae55-fa428bb12165	211a5fb2-b404-4b0e-91d1-dec2c431d422	Lrmyoa/94oCaD70HODlDXI6MButB3+dCHXAcPPNFsNXnF9ojlSHhtFIu/C7S88dRCcY0tg1D0bPmIKCU0fxtIA==	2026-07-19 17:44:52.887765	f	2026-06-19 17:44:52.887817
\.


--
-- Data for Name: user_devices; Type: TABLE DATA; Schema: public; Owner: flowfi
--

COPY public.user_devices (id, user_id, device_id, device_name, platform, push_token, last_synced_at, created_at) FROM stdin;
33333333-3333-3333-3333-333333333333	aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa	device_iphone_15	iPhone 15 Pro	IOS	apns_token_here	\N	2026-06-19 17:42:35.330923
44444444-4444-4444-4444-444444444444	bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb	device_samsung_s24	Samsung S24	ANDROID	fcm_token_here	\N	2026-06-19 17:42:35.330923
\.


--
-- Data for Name: user_logs; Type: TABLE DATA; Schema: public; Owner: flowfi
--

COPY public.user_logs (id, user_id, action, status, ip_address, user_agent, failure_reason, created_at) FROM stdin;
55555555-5555-5555-5555-555555555555	aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa	LOGIN	SUCCESS	192.168.1.1	Mozilla/5.0	\N	2026-06-19 17:42:35.334492
66666666-6666-6666-6666-666666666666	aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa	LOGIN	FAILED	192.168.1.1	Mozilla/5.0	WRONG_PASSWORD	2026-06-19 17:42:35.334492
40d2799d-f9f5-409d-8a52-dd9b3ae30325	211a5fb2-b404-4b0e-91d1-dec2c431d422	REGISTER	SUCCESS	\N	\N	\N	2026-06-19 17:44:52.924756
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: flowfi
--

COPY public.users (id, email, password_hash, full_name, avatar_url, date_of_birth, currency_code, monthly_budget_limit, auth_provider, is_verified, created_at, updated_at, deleted_at) FROM stdin;
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa	nguyenvana@gmail.com	$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.UFMpA8E1P5DXLK	Nguyen Van A	\N	\N	VND	5000000.00	LOCAL	t	2026-06-19 17:42:35.311115+00	\N	\N
bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb	tranvanb@gmail.com	\N	Tran Van B	\N	\N	USD	500.00	GOOGLE	t	2026-06-19 17:42:35.311115+00	\N	\N
211a5fb2-b404-4b0e-91d1-dec2c431d422	test2@gmail.com	qJ45NJz0SX9jmdmyrApXew==.fzcuGhlTarB751kXtZXsAzTNcPFGsnbBmqDh4R0D9wg=.100000	Test User 2	\N	\N	VND	\N	LOCAL	f	2026-06-19 17:44:52.790172+00	\N	\N
\.


--
-- Name: password_reset_tokens password_reset_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.password_reset_tokens
    ADD CONSTRAINT password_reset_tokens_pkey PRIMARY KEY (id);


--
-- Name: refresh_tokens refresh_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT refresh_tokens_pkey PRIMARY KEY (id);


--
-- Name: user_devices user_devices_pkey; Type: CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.user_devices
    ADD CONSTRAINT user_devices_pkey PRIMARY KEY (id);


--
-- Name: user_logs user_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.user_logs
    ADD CONSTRAINT user_logs_pkey PRIMARY KEY (id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: idx_password_reset_tokens_token; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_password_reset_tokens_token ON public.password_reset_tokens USING btree (token);


--
-- Name: idx_password_reset_tokens_user_id; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_password_reset_tokens_user_id ON public.password_reset_tokens USING btree (user_id);


--
-- Name: idx_refresh_tokens_token; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_refresh_tokens_token ON public.refresh_tokens USING btree (token);


--
-- Name: idx_refresh_tokens_user_id; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_refresh_tokens_user_id ON public.refresh_tokens USING btree (user_id);


--
-- Name: idx_user_devices_device_id; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_user_devices_device_id ON public.user_devices USING btree (device_id);


--
-- Name: idx_user_devices_user_id; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_user_devices_user_id ON public.user_devices USING btree (user_id);


--
-- Name: idx_user_logs_action; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_user_logs_action ON public.user_logs USING btree (action);


--
-- Name: idx_user_logs_created_at; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_user_logs_created_at ON public.user_logs USING btree (created_at);


--
-- Name: idx_user_logs_user_id; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_user_logs_user_id ON public.user_logs USING btree (user_id);


--
-- Name: idx_users_auth_provider; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_users_auth_provider ON public.users USING btree (auth_provider);


--
-- Name: idx_users_email; Type: INDEX; Schema: public; Owner: flowfi
--

CREATE INDEX idx_users_email ON public.users USING btree (email);


--
-- Name: password_reset_tokens fk_password_reset_tokens_users; Type: FK CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.password_reset_tokens
    ADD CONSTRAINT fk_password_reset_tokens_users FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: refresh_tokens fk_refresh_tokens_users; Type: FK CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_users FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_devices fk_user_devices_users; Type: FK CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.user_devices
    ADD CONSTRAINT fk_user_devices_users FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_logs fk_user_logs_users; Type: FK CONSTRAINT; Schema: public; Owner: flowfi
--

ALTER TABLE ONLY public.user_logs
    ADD CONSTRAINT fk_user_logs_users FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict knqlced6AarPXaiH4tRZjazywHZBBxSieAzKWBgyumxtlYmrr7gcETKZAnJHs4q

