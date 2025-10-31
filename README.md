# LiveStock Management System

A comprehensive livestock management system built with ASP.NET Core, designed for modern farm operations. This system provides both a public-facing agriculture website and an internal management system for tracking livestock, staff, tasks, and farm operations.

## Link to the Azure hosted website: https://live-stock.azurewebsites.net/

## 🌾 Features

### Public Website
- **Modern Agriculture Branding**: Clean, professional design with natural color palette
- **Responsive Design**: Optimized for desktop and mobile devices
- **Hero Section**: "Cultivating the Future of Farming" messaging
- **Feature Blocks**: Sustainable practices, renewable sourcing, and expertise
- **Services Overview**: Crop management, livestock farming, sustainable practices

### Internal Management System
- **Sheep Management**: Add, edit, track medical records, monitor placement
- **Cow Management**: Comprehensive cattle tracking with pregnancy monitoring
- **Camp Management**: 15-camp system with movement tracking
- **Staff Management**: Employee profiles, communication, task assignment
- **Task Management**: Create, assign, and track tasks with deadlines
- **Water & Rainfall Tracking**: Monitor rainfall across all camps
- **Assets & Finance**: Track farm assets and financial records
- **Medical Records**: Comprehensive health tracking for all livestock

## 🎨 Design

The system uses the exact color scheme from the Figma design:
- **Dark Green**: #13341E
- **Medium Green**: #4E7145
- **Orange-Yellow**: #EB9C35
- **Burnt Orange**: #C4501B
- **Darker Brown**: #8A2B13

## 🏗️ Architecture

- **LiveStock.Core**: Domain models and business logic
- **LiveStock.Infrastructure**: Data access and Entity Framework with SQLite
- **LiveStock.Web**: MVC web application
- **LiveStock.API**: REST API (for future mobile app integration)

**Built with .NET 9.0** - Latest and greatest framework for modern web applications
**Database**: SQLite for cross-platform compatibility (Windows, macOS, Linux)

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQLite (built-in, no additional installation needed)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd LiveStock
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Database is automatically configured**
   - Uses SQLite with automatic database creation
   - No additional database setup required

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   cd LiveStock.Web
   dotnet run
   ```

6. **Access the application**
   - Public Website: `https://localhost:5001`
   - Login: Use demo credentials below

### Demo Credentials
- **Employee ID**: `FARM001`
- **Password**: `demo123`

## 📱 Usage

### Public Website
1. Navigate to the homepage to see the agriculture branding
2. Explore the features and services sections
3. Click "Login" to access the management system

### Management System
1. **Dashboard**: Overview of farm statistics and quick actions
2. **Sheep Management**: Add, search, filter, and track sheep
3. **Cow Management**: Manage cattle with detailed tracking
4. **Camps**: View and manage 15 farm camps
5. **Staff**: Manage employees and assign tasks
6. **Tasks**: Create, assign, and monitor farm tasks
7. **Water & Rainfall**: Track precipitation across camps
8. **Assets & Finance**: Monitor farm resources and finances

## 🗄️ Database

The system uses Entity Framework Core with SQL Server. The database includes:
- **Animals**: Sheep and cows with detailed tracking
- **Camps**: 15 farm locations with capacity management
- **Staff**: Employee management and communication
- **Tasks**: Work assignment and tracking
- **Medical Records**: Health and treatment history
- **Camp Movements**: Livestock relocation tracking
- **Rainfall Records**: Water monitoring data
- **Assets**: Farm equipment and resources
- **Financial Records**: Income and expense tracking

## 🔧 Configuration

### Environment Variables
- `DefaultConnection`: Database connection string
- `Logging`: Log level configuration

### Customization
- Modify colors in `wwwroot/css/site.css`
- Update farm details in `Views/Home/Index.cshtml`
- Adjust camp count in `LiveStockDbContext.cs`

## 📊 Farm Specifications

- **Total Area**: 2,300 hectares
- **Number of Camps**: 15
- **Camp Size**: ~153.33 hectares each
- **Livestock Types**: Sheep and Cattle
- **Staff Roles**: Farmer and Staff members

## 🚧 Future Enhancements

- **Mobile Application**: React Native or Flutter app
- **Real-time Notifications**: SMS and email alerts
- **Advanced Analytics**: Farm performance metrics
- **Weather Integration**: Real-time weather data
- **GPS Tracking**: Livestock location monitoring
- **Inventory Management**: Feed and supply tracking

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support or questions, please contact the development team or create an issue in the repository.

---

**Built with ❤️ for modern agriculture** 
