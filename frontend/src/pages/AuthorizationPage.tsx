import { useState } from 'react';

import { useNavigate } from 'react-router-dom';
import * as Yup from 'yup';
import { Formik, Form, Field, ErrorMessage, FormikHelpers } from 'formik';

import { useAuth } from '@/providers/auth';

type LoginValues = {
	email: string;
	password: string;
};

type SignupValues = {
	firstName: string;
	lastName: string;
	email: string;
	password: string;
	confirmPassword: string;
};

const loginSchema = Yup.object<LoginValues>({
	email: Yup.string().email('Invalid email').required('Email is required'),
	password: Yup.string().required('Password is required'),
});

const signupSchema = Yup.object<SignupValues>({
	firstName: Yup.string().required('First name is required'),
	lastName: Yup.string().required('Last name is required'),
	email: Yup.string().email('Invalid email').required('Email is required'),
	password: Yup.string()
		.min(8, 'Password must be at least 8 characters')
		.required('Password is required'),
	confirmPassword: Yup.string()
		.oneOf([Yup.ref('password')], 'Passwords must match')
		.required('Confirm password is required'),
});

export const AuthorizationPage = () => {
	const { login } = useAuth();
	const navigate = useNavigate();
	const [mode, setMode] = useState<'login' | 'signup'>('login');

	const handleAuthSuccess = () => {
		const to = sessionStorage.getItem('redirectAfterLogin') || '/';
		sessionStorage.removeItem('redirectAfterLogin');
		navigate(to, { replace: true });
	};

	return (
		<div
			style={{
				maxWidth: 400,
				margin: '5vh auto',
				padding: '2rem',
				border: '1px solid #e0e0e0',
				borderRadius: 8,
				boxShadow: '0 2px 10px rgba(0,0,0,0.1)',
			}}
		>
			<div style={{ display: 'flex', marginBottom: '1rem' }}>
				{['login', 'signup'].map((tab) => (
					<button
						key={tab}
						onClick={() => setMode(tab as 'login' | 'signup')}
						style={{
							flex: 1,
							padding: '0.5rem 0',
							background: mode === tab ? '#1976d2' : 'white',
							color: mode === tab ? 'white' : '#1976d2',
							border: '1px solid #1976d2',
							cursor: 'pointer',
						}}
					>
						{tab === 'login' ? 'Log In' : 'Sign Up'}
					</button>
				))}
			</div>

			{mode === 'login' ? (
				<Formik<LoginValues>
					initialValues={{ email: '', password: '' }}
					validationSchema={loginSchema}
					onSubmit={(values, { setSubmitting }: FormikHelpers<LoginValues>) => {
						login(values.email, values.password);
						setSubmitting(false);
						handleAuthSuccess();
					}}
				>
					{({ isSubmitting }) => (
						<Form
							style={{
								display: 'flex',
								flexDirection: 'column',
								gap: '0.75rem',
							}}
						>
							<div>
								<Field
									name="email"
									type="email"
									placeholder="Email"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="email" />
								</div>
							</div>
							<div>
								<Field
									name="password"
									type="password"
									placeholder="Password"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="password" />
								</div>
							</div>
							<button
								type="submit"
								disabled={isSubmitting}
								style={{
									padding: '0.75rem',
									background: '#1976d2',
									color: 'white',
									border: 'none',
									cursor: 'pointer',
								}}
							>
								{isSubmitting ? 'Logging in…' : 'Log In'}
							</button>
						</Form>
					)}
				</Formik>
			) : (
				<Formik<SignupValues>
					initialValues={{
						firstName: '',
						lastName: '',
						email: '',
						password: '',
						confirmPassword: '',
					}}
					validationSchema={signupSchema}
					onSubmit={(
						values,
						{ setSubmitting }: FormikHelpers<SignupValues>
					) => {
						login(values.email, values.password);
						setSubmitting(false);
						handleAuthSuccess();
					}}
				>
					{({ isSubmitting }) => (
						<Form
							style={{
								display: 'flex',
								flexDirection: 'column',
								gap: '0.75rem',
							}}
						>
							<div>
								<Field
									name="firstName"
									placeholder="First Name"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="firstName" />
								</div>
							</div>
							<div>
								<Field
									name="lastName"
									placeholder="Last Name"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="lastName" />
								</div>
							</div>
							<div>
								<Field
									name="email"
									type="email"
									placeholder="Email"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="email" />
								</div>
							</div>
							<div>
								<Field
									name="password"
									type="password"
									placeholder="Password"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="password" />
								</div>
							</div>
							<div>
								<Field
									name="confirmPassword"
									type="password"
									placeholder="Confirm Password"
									style={{ width: '100%', padding: '0.5rem' }}
								/>
								<div style={{ color: 'red', fontSize: '0.85rem' }}>
									<ErrorMessage name="confirmPassword" />
								</div>
							</div>
							<button
								type="submit"
								disabled={isSubmitting}
								style={{
									padding: '0.75rem',
									background: '#1976d2',
									color: 'white',
									border: 'none',
									cursor: 'pointer',
								}}
							>
								{isSubmitting ? 'Creating account…' : 'Sign Up'}
							</button>
						</Form>
					)}
				</Formik>
			)}
		</div>
	);
};
