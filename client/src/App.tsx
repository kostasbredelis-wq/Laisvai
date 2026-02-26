import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './routes/ProtectedRoute'

// Placeholder pages — to be replaced in subsequent prompts
const Home = () => <div className="p-8 text-2xl font-bold">Laisvai</div>
const Login = () => <div className="p-8">Login</div>
const Register = () => <div className="p-8">Register</div>
const FreelancerDashboard = () => <div className="p-8">Freelancer Dashboard</div>
const ClientDashboard = () => <div className="p-8">Client Dashboard</div>
const AdminDashboard = () => <div className="p-8">Admin Dashboard</div>
const NotFound = () => <div className="p-8">404 — Page not found</div>

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/freelancers" element={<Home />} />
          <Route path="/freelancers/:id" element={<Home />} />
          <Route path="/jobs" element={<Home />} />

          {/* Freelancer */}
          <Route path="/dashboard" element={
            <ProtectedRoute roles={['Freelancer']}>
              <FreelancerDashboard />
            </ProtectedRoute>
          } />

          {/* Client */}
          <Route path="/client/dashboard" element={
            <ProtectedRoute roles={['Client']}>
              <ClientDashboard />
            </ProtectedRoute>
          } />

          {/* Admin */}
          <Route path="/admin" element={
            <ProtectedRoute roles={['Admin']}>
              <AdminDashboard />
            </ProtectedRoute>
          } />

          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
