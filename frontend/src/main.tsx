import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { appInsights } from './appInsights';
import { AppInsightsErrorBoundary } from './components';
import './index.css';
import { AuthProvider, ReactQueryProvider } from './providers';
import { I18nTranslateProvider } from './providers/i18n-translate-provider';

appInsights.loadAppInsights();

createRoot(document.getElementById('root')!).render(
	<StrictMode>
		<BrowserRouter>
			<I18nTranslateProvider>
				<ReactQueryProvider>
					<AuthProvider>
						<AppInsightsErrorBoundary boundaryName="FrontendRootApp">
							<App />
						</AppInsightsErrorBoundary>
					</AuthProvider>
				</ReactQueryProvider>
			</I18nTranslateProvider>
		</BrowserRouter>
	</StrictMode>
);
