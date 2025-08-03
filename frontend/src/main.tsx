import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import {
	ReactQueryProvider,
	I18nTranslateProvider,
	AuthProvider,
} from './providers';
import './index.css';
import App from './App.tsx';
import { AppInsightsErrorBoundary } from './components';
import { appInsights } from './appInsights';

appInsights.loadAppInsights();

createRoot(document.getElementById('root')!).render(
	<StrictMode>
		<I18nTranslateProvider>
			<ReactQueryProvider>
				<AuthProvider>
					<AppInsightsErrorBoundary boundaryName="FrontendRootApp">
						<App />
					</AppInsightsErrorBoundary>
				</AuthProvider>
			</ReactQueryProvider>
		</I18nTranslateProvider>
	</StrictMode>
);
